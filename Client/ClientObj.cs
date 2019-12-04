using MeetingCalendar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace Client
{
    public class ClientObj : MarshalByRefObject, IClientServices
    {
        private List<string> myCreatedMeetings = new List<string>();
        private List<IMeetingServices> meetingsClientKnows = new List<IMeetingServices>();
        private string userName;
        private List<string> otherServerURLs = new List<string>();
        private List<IServerServices> otherServers = new List<IServerServices>();
        ServerServices myServer;
        private string serverURL;
        private string myURL;
        TcpChannel tcp;

        public ClientObj(string userName, string clientURL, string serverURL, string scriptFileName)
        {
            this.userName = userName;
            this.myURL = clientURL;
          

            
            //this.RunScript(scriptFileName);
        }
        private void initialize(string username, string clientURL,string serverURL, string scriptFileName, ClientObj client )
        {
            string[] partlyURL = clientURL.Split(':');
            string[] endURL = partlyURL[partlyURL.Length - 1].Split('/');
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            //props["port"] = 8085;
            props["port"] = Int32.Parse(endURL[0]);
            tcp = new TcpChannel(props, null, provider);

            //Setup the client singleton
            Console.WriteLine("Client obj at: " + clientURL);
            ChannelServices.RegisterChannel(tcp, false);
            //RemotingConfiguration.RegisterWellKnownServiceType(
            //   typeof(ClientObj),
            //  userName,
            // WellKnownObjectMode.Singleton);
            RemotingServices.Marshal(client, userName, typeof(ClientObj));


            //Setup my server
            Console.WriteLine("Creates connection to Server obj at: " + serverURL);
            this.myServer = (ServerServices)Activator.GetObject(
            typeof(ServerServices),
            serverURL);
            myServer.NewClient(this.userName, clientURL);


            //Set up other servers
            this.setupOtherServers(myServer.getMaxFaults(), myServer, clientURL);

            this.RunScript(scriptFileName);
        }

        private void setupOtherServers(int maxFaults, ServerServices server, string clientURL)
        {
            List<string> servers = server.getOtherServerURLs();
         
           
            Console.WriteLine("There are [" + servers.Count + "] servers in the system other than " + server.getServerURL());
        

            if (servers.Count < 1) //No other servers yet
            {
                //throw new Exception("Can not find a new server to connect to"); 
            }
            if (servers.Count >= maxFaults)
            {
                for (int i = 0; i < maxFaults; i++)
                {
                    this.otherServerURLs.Add(servers[i]);

                }
            }
            else
            {
                for (int i = 0; i < servers.Count; i++)
                {
                    this.otherServerURLs.Add(servers[i]);
                }
            }
  
        }

        public void RunScript(string scriptFileName)
        {
            CultureInfo ci = new CultureInfo("pt-PT");
            string[] command;
            string[] lines = System.IO.File.ReadAllLines(scriptFileName);
            foreach(String line in lines)
            {
                command = line.Split(' ');
                switch (command[0])
                {
                    case "list":
                        this.ListMeetings();
                        break;
                    case "create":
                        string topic = command[1];
                        int minAttendees = Int32.Parse(command[2]);
                        int numberOfSlots = Int32.Parse(command[3]);
                        int numInvitees = Int32.Parse(command[4]);
                        List<(string, DateTime)> dateLoc = new List<(string, DateTime)>();
                        List<string> clientInvites = new List<string>();
                        for (int i = 5; i < 5+numberOfSlots; i++)
                        {
                            string[] locDate = command[i].Split(',');
                            dateLoc.Add((locDate[0], DateTime.Parse(locDate[1], ci)));
                        }
                        if (numInvitees != 0)
                        {
                            for (int j = 5 + numberOfSlots; j < numInvitees; j++)
                            {
                                clientInvites.Add(command[j]);
                            }
                            
                        } else
                        {
                            clientInvites = null;
                        }
                        this.createMeeting(topic, minAttendees,
                            dateLoc, clientInvites);
                        break;
                    case "join":
                        string meetingTopic = command[1];
                        int numSlots = Int32.Parse(command[2]);
                        List<(string, DateTime)> dateLoc2 = new List<(string, DateTime)>();
                        for (int k = 3; k < 3+numSlots; k++)
                        {
                            string[] locDate = command[k].Split(',');
                            dateLoc2.Add((locDate[0], DateTime.Parse(locDate[1], ci)));
                        }
                        this.JoinMeeting(meetingTopic, dateLoc2);
                        break;
                    case "close":
                        this.closeMeetingProposal(command[1]);
                        break;
                    case "wait":
                        this.wait(Int32.Parse(command[1]));
                        break;
                    default:
                        break;
                }
                Console.WriteLine("Press any key for next command");
                Console.ReadLine();
            }
            
        }

        private List<string> ListInvitees(string[] command)
        {
            int m = Int32.Parse(command[4]);
            int n = Int32.Parse(command[3]);
            List<string> invitees = new List<string>();
            for (int j = 5+n ; j < 5+n + m; j++) {
                invitees.Add(command[j]);
            }
            return invitees;
        }

        private List<(string, DateTime)> ParseDateLoc(string[] command, int n)
        {
            List<(string, DateTime)> slots = new List<(string, DateTime)>();
            for (int i = 5; i < 5 + n; i++)
            {
                string[] dateLoc = command[i].Replace(@"(", "").Replace(@")", "").Split(';');
                slots.Add((dateLoc[0], DateTime.Parse(dateLoc[1])));
            }
            return slots;
        }

        private void wait(int delayTime)
        {
            // Not sure if works, temporary solution
            Thread.Sleep(delayTime);
        }
        private void createMeeting(string meetingTopic, int minAttendees,
            List<(string, DateTime)> slots, List<string> invitees)
        {
            try
            {
                IMeetingServices meetingProposal = new MeetingServices(this.userName, meetingTopic, minAttendees, slots, invitees);
                meetingsClientKnows.Add(meetingProposal);
                myServer.newMeetingProposal(meetingProposal);

                //Dersom motet skal sendes til alle, uten gjesteliste
                if (invitees == null)
                {
                    List<string> clientInSameHuB = getClientsInSameHub();
                    List<string> sampleClientsFromOtherServers = getSampleClients();
                    
                    if (clientInSameHuB != null)
                    {
                        Console.WriteLine("Clients in same hub:");
                        foreach (string client in clientInSameHuB)
                        {
                            Console.WriteLine(client);
                        }
                        informOtherClients(meetingProposal, clientInSameHuB, false);
                    }
                    
                    if (sampleClientsFromOtherServers != null)
                    {
                        Console.WriteLine("Sample of clients from other servers: ");
                        foreach (string client in sampleClientsFromOtherServers)
                        {
                            Console.WriteLine(client);
                        };
                        
                        informOtherClients(meetingProposal, sampleClientsFromOtherServers, true);
                    }
                }
                myCreatedMeetings.Add(meetingTopic);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                changeServer();
                createMeeting(meetingTopic, minAttendees, slots, invitees);
            }
        }

        private void informOtherClients(IMeetingServices meetingProposal, List<string> clientURLs, bool forwardMeeting)
        {
            if (clientURLs.Count > 0) { 
                List<ClientObj> clients = new List<ClientObj>();
                foreach (string URL in clientURLs){
                    ClientObj client = (ClientObj)Activator.GetObject(
                    typeof(ClientObj),
                    URL);
                    clients.Add(client);
                }
                foreach (ClientObj client in clients)
                {
                    client.receiveNewMeeting(meetingProposal, forwardMeeting);
                }
            }  
        }

        public void receiveNewMeeting(IMeetingServices meetingProposal, bool forwardMeeting)
        {
            if (!meetingsClientKnows.Contains(meetingProposal)){
                meetingsClientKnows.Add(meetingProposal);
            }
            
            Console.WriteLine("Meetingproposal received. I am "+ this.userName );
            if (forwardMeeting)
            {
                try
                {
                    List<string> clientsFromSameServer = getClientsInSameHub();
                    foreach (string client in clientsFromSameServer)
                    {
                        Console.WriteLine("I am client " + this.userName +"    CLient from same server" + client);
                    }
                    if (clientsFromSameServer != null)
                    {
                         informOtherClients(meetingProposal, clientsFromSameServer, false);
                    }  
                }
                catch (Exception e)
                {
                    Console.WriteLine("CHANGING SERVER!");
                    this.changeServer();
                }
            }
        }

        private List<string> getClientsInSameHub()
        {
            try
            {
               List<string> clientsInSameHub = myServer.getOwnClients();
                if (clientsInSameHub.Contains(this.myURL))
                {
                    clientsInSameHub.Remove(this.myURL);

                }
                return clientsInSameHub;
            }
            catch(Exception e)
            {
                this.changeServer();
                Console.WriteLine("CHANGING SERVER!");
                Console.WriteLine("Failed when getting clients from same hub. Error message: " + e);
            }
            return null;
        }
        private List<string> getSampleClients()
        {
            try
            {
                List<string> sample = myServer.getSampleClientsFromOtherServers();
                if (sample == null)
                {
                    Console.WriteLine("No other clients in other servers.");
                    return null;
                }
                return sample;
            }catch (Exception e)
            {
                this.changeServer();
                Console.WriteLine("CHANGING SERVER!");
                Console.WriteLine("Failed while getting clients from other servers. Error message: " + e);
            }
            return null;
        }

        private void JoinMeeting(string meetingTopic, List<(string, DateTime)> dateLoc)
        {
            try
            {
                myServer.JoinMeeting(meetingTopic, this.userName, true, dateLoc);
            } catch (Exception e) {
                Console.WriteLine(e);
                Console.WriteLine("CHANGING SERVER!");

                changeServer();
                JoinMeeting(meetingTopic, dateLoc);
            }
        }

        private void closeMeetingProposal(string meetingTopic)
        {
            if (myCreatedMeetings.Contains(meetingTopic))
            {
                try
                {
                   bool mybool = myServer.closeMeetingProposal(meetingTopic, this.userName);
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("CHANGING SERVER!");
                    changeServer();
                    closeMeetingProposal(meetingTopic);
                }
            }
        }

        private void ListMeetings()
        {
            try
            {
                List<IMeetingServices> availableMeetings = myServer.ListMeetings(userName, meetingsClientKnows, true);
                foreach (MeetingServices meeting in availableMeetings)
                {
                    string topic = meeting.getTopic();
                    List<(string, DateTime)> locDateOptions = meeting.LocDateOptions;
                    string coordinatorUsername = meeting.CoordinatorUsername;
                    bool isClosed = meeting.isClosed;
                    Dictionary<(string, DateTime), List<string>> participants = meeting.GetParticipants;

                    Console.WriteLine("Dates and locations set by the coordinator: ");
                    foreach ((string, DateTime) dateLoc in locDateOptions)
                    {
                        Console.WriteLine("Place: " + dateLoc.Item1 + ",  date: " + dateLoc.Item2.ToString("d"));
                    }
                    Console.WriteLine("Meeting topic: " + topic);
                    Console.WriteLine("Meeting closed: " + isClosed);
                    //Console.WriteLine("Possible places and dates: ");
                    foreach (KeyValuePair<(string, DateTime), List<string>> probMeeting in participants)
                    {
                        Console.WriteLine("Location: " + probMeeting.Key.Item1 + ", Date: " + probMeeting.Key.Item2 +
                            ", number of people who can attend at this date and place: " + probMeeting.Value.Count);
                    }
                    Console.WriteLine("Coordinator of meeting: " + coordinatorUsername);

                }
            } catch (Exception e)
            {
                Console.WriteLine("Could not list meetings...!");
                Console.WriteLine(e);
                Console.WriteLine("CHANGING SERVER!");
                changeServer();
                ListMeetings();
                
               
            }
        }

        private void changeServer()
        {
            if (this.otherServerURLs.Count > 0)
            {
                string failedServerURL = serverURL;
                ServerServices s = (ServerServices)Activator.GetObject(
                    typeof(ServerServices),
                   otherServerURLs[0]);
                this.myServer = s;
                this.serverURL = s.getServerURL();
                otherServerURLs.RemoveAt(0);
                s.NewClient(this.userName, this.myURL);
                myServer.failedServerDetected(failedServerURL);
            }
        }

        public void PrintStatus()
        {
            Console.WriteLine("I am client: " + userName + ". My server is " + myServer.getServerURL() + ".");
            foreach (string s in this.otherServerURLs)
            {
                Console.WriteLine("My server urls are " + s);
            }
        }
        static void Main(string[] args)
        {
            ClientObj co = new ClientObj(args[0], args[1], args[2], args[3]);
            co.initialize(args[0], args[1], args[2],args[3], co);
            co.PrintStatus();
            
            Console.WriteLine("<enter> to exit...");
            Console.ReadLine();
        }
    }
}
