﻿using MeetingCalendar;
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
        private void initialize(string username, string clientURL,string serverURL, ClientObj client )
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
            Console.WriteLine("Creates connection to Server obj at: " + serverURL);
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

            //this.RunScript(scriptFileName);
            if (userName == "client4")
            {
                DateTime slotdate = new DateTime(2018, 02, 04);
                List<(string, DateTime)> slots = new List<(string, DateTime)> { ("Lisbon", slotdate) };
                this.createMeeting("bestTopic", 2, slots, null);

            }
            
        }

        private void setupOtherServers(int maxFaults, ServerServices server, string clientURL)
        {
            List<string> servers = server.getOtherServerURLs();
         
           
            Console.WriteLine("There are [" + servers.Count + "] servers in the system other than " + server.getServerURL());
        

            if (servers.Count < 1) //No other servers yet
            {
                throw new Exception("Can not find a new server to connect to"); 
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

                //Dersom motet skal sendes til alle, uten gjesteliste
                if (invitees == null)
                {
                 informOtherClients(meetingProposal, getClientsInSameHub(), false);
                 informOtherClients(meetingProposal, getSampleClients(), true);
    
                }
               

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                this.changeServer();
            }
            // Create new meeting
            // USE TRY-CATCH
        }

        private void informOtherClients(IMeetingServices meetingProposal, List<string> clientURLs, bool forwardMeeting)
        {
            List<IClientServices> clients = new List<IClientServices>();
            foreach (string URL in clientURLs){
                Console.WriteLine("Making client object with URL: " + URL);
                ClientObj client = (ClientObj)Activator.GetObject(
                typeof(ClientObj),
                URL);
                clients.Add(client);
            }
            foreach (IClientServices client in clients)
            {
                Console.WriteLine("Looping client and receive");
                client.receiveNewMeeting(meetingProposal, forwardMeeting);
                
            }
        }

        public void receiveNewMeeting(IMeetingServices meetingProposal, bool forwardMeeting)
        {
            Console.WriteLine("Reached receiveNewMeeting-method");
            meetingsClientKnows.Add(meetingProposal);
            Console.WriteLine("Meetingproposal received");
            if (forwardMeeting)
            {
                try
                {
                    List<string> clientsFromSameServer = myServer.getOwnClients();
                    informOtherClients(meetingProposal, clientsFromSameServer, false);
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
                Console.WriteLine("Clients in same hub:   "+ clientsInSameHub);
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
                    Console.WriteLine("CHANGING SERVER!");
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
                Console.WriteLine("CHANGING SERVER!");
                this.changeServer();
            }
        }

        private void changeServer()
        {
            if (this.otherServerURLs.Count > 0)
            {
                ServerServices s = (ServerServices)Activator.GetObject(
                    typeof(ServerServices),
                   otherServerURLs[0]);
                this.myServer = s;
                s.NewClient(this.userName, this.myURL);
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
            co.initialize(args[0], args[1], args[2],co);
            co.PrintStatus();
            
            Console.WriteLine("<enter> to exit...");
            Console.ReadLine();
        }
    }
}
