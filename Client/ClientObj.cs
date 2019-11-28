using MeetingCalendar;
using System;
using System.Collections;
using System.Collections.Generic;
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
            Console.WriteLine("Creates connection to Server obj at: " + serverURL);
            ChannelServices.RegisterChannel(tcp, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ClientObj),
                userName,
                WellKnownObjectMode.Singleton);

            //Setup my server
            Console.WriteLine("Creates connection to Server obj at: " + serverURL);
            this.myServer = (ServerServices)Activator.GetObject(
                typeof(ServerServices),
                serverURL);
            myServer.NewClient(this.userName, clientURL);

            //Set up other servers
            this.setupOtherServers(myServer.getMaxFaults(), myServer, clientURL);

            //this.RunScript(scriptFileName);
        }

        private void setupOtherServers(int maxFaults, ServerServices server, string clientURL)
        {
            List<string> servers = server.getOtherServerURLs();
            List<IServerServices> serverInstances = server.Servers;
           
            Console.WriteLine("There are [" + servers.Count + "] servers in the system other than " + server.getServerURL());
            Console.WriteLine("There are [" + serverInstances.Count + "] serverInstances in the system other than " + server);

            if (servers.Count < 1) //No other servers yet
            {
                //throw new Exception("Can not find a new server to connect to"); 
            }
            if (servers.Count >= maxFaults)
            {
                for (int i = 0; i < maxFaults; i++)
                {
                    this.otherServerURLs.Add(servers[i]);

                    ServerServices s = (ServerServices)Activator.GetObject(
                    typeof(ServerServices),
                    servers[i]);
                    this.otherServers.Add(s);
                    s.NewClient(this.userName, clientURL);
                }
            }
            else
            {
                for (int i = 0; i < servers.Count; i++)
                {
                    this.otherServerURLs.Add(servers[i]);

                    ServerServices s = (ServerServices)Activator.GetObject(
                    typeof(ServerServices),
                    servers[i]);
                    this.otherServers.Add(s);
                    s.NewClient(this.userName, clientURL);
                }
            }
        }

        public void RunScript(string scriptFileName)
        {
            
            string[] command;
            StreamReader script = new StreamReader(scriptFileName);
            while((command = script.ReadLine().Split(',')) != null)
            {
                switch (command[0])
                {
                    case "list":
                        this.ListMeetings();
                        break;
                    case "create":
                        this.createMeeting(command[1], Int32.Parse(command[2]),
                            this.ParseDateLoc(command, Int32.Parse(command[3])), this.ListInvitees(command));
                        break;
                    case "join":
                        this.JoinMeeting(command[1], this.ParseDateLoc(command, Int32.Parse(command[2])));
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
            System.Threading.Thread.Sleep(delayTime);
        }
        private void createMeeting(string meetingTopic, int minAttendees,
            List<(string, DateTime)> slots, List<string> invitees)
        {
            try
            {
            IMeetingServices meetingProposal = new MeetingServices(this.userName, meetingTopic, minAttendees, slots, invitees);
                meetingsClientKnows.Add(meetingProposal);
                myServer.NewMeetingProposal(meetingProposal);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                this.changeServer();
            }
            // Create new meeting
            // USE TRY-CATCH
        }


        private List<string> getListOfClientURLs()
        {
            List<string> sample = myServer.getSampleClientsFromOtherServers();
            List<string> clientsFromSameServer = myServer.getOwnClients();
            if (clientsFromSameServer.Contains(this.myURL)) {
                clientsFromSameServer.Remove(this.myURL); 
            }
            return sample.Concat(clientsFromSameServer).ToList();
        }

        private void JoinMeeting(string meetingTopic, List<(string, DateTime)> dateLoc)
        {
            try
            {
                myServer.JoinMeeting(meetingTopic, this.userName, true, dateLoc);
            } catch (Exception e) {
                Console.WriteLine(e);
                this.changeServer();
            }
        }

        private void closeMeetingProposal(string meetingTopic)
        {
            if (myCreatedMeetings.Contains(meetingTopic))
            {
                try
                {
                    myServer.closeMeetingProposal(meetingTopic, this.userName);
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                    changeServer();
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
                    meeting.printStatus();
                }
            } catch (Exception e)
            {
                Console.WriteLine("Could not list meetings...!");
                Console.WriteLine(e);
                this.changeServer();
            }
        }

        private void changeServer()
        {
            if (this.otherServers.Count > 0)
            {
                this.myServer = (ServerServices)otherServers[0];
            }
        }

        public void PrintStatus()
        {
            Console.WriteLine("I am client: " + userName + ". My server is " + myServer.getServerURL() + ".");
            foreach (string s in this.otherServerURLs)
            {
                Console.WriteLine("My server urls are " + s);
            }
            foreach (IServerServices serverConnection in this.otherServers)
            {
                Console.WriteLine("My server connections are " + serverConnection.getServerURL());
            }
        }
        static void Main(string[] args)
        {
            ClientObj co = new ClientObj(args[0], args[1], args[2], args[3]);
            co.PrintStatus();

            Console.WriteLine("<enter> to exit...");
            Console.ReadLine();
        }
    }
}
