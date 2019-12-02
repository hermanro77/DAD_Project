﻿using CommonTypes;
using Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using static CommonTypes.CommonType;

namespace MeetingCalendar
{
    public class ServerServices : MarshalByRefObject, IServerServices, IEquatable<ServerServices>
    {
        private Dictionary<string, IClientServices> clients = new Dictionary<string, IClientServices>();
        private List<IServerServices> otherServers = new List<IServerServices>();
        private List<string> otherServerURLs = new List<string>();
        private List<string> clientURLs = new List<string>();
        private List<IMeetingServices> meetings = new List<IMeetingServices>();
        private Location location = new Location();
        private int millSecWait;
        private string myServerURL;
        private string serverID;
        private int max_faults;
        private bool frozenMode = false;
        private bool isSequencer;
        private int seqNr;
        private IServerServices sequencer;

        TcpChannel channel;
        private Random rnd = new Random();

      
        public ServerServices(string otherServerURL, string serverID, string serverURL, int max_faults,
            int minWait, int maxWait)
        {
            this.serverID = serverID;
            this.millSecWait = (minWait == 0 && maxWait == 0) ? 0 : rnd.Next(minWait, maxWait);
            this.myServerURL = serverURL;
            this.max_faults = max_faults;
            if (otherServerURL != null && otherServerURL.Contains("tcp")) //if it's not the first server created find all other servers in system
            {
                setAllOtherServers(otherServerURL); //uses otherServerURL to get all servers currently set up and add them to serverURLs. 
                sequencer = otherServers[0];
                isSequencer = false;
            }
            else
            {
                //sets sequencer to be myself
                isSequencer = true;
                sequencer = null;
                seqNr = 0;
                
            }
        }

        private void initialize(string serverURL, string serverID, ServerServices serverObj)
        {
            string[] partlyURL = serverURL.Split(':');
            string[] endURL = partlyURL[partlyURL.Length - 1].Split('/');
            Console.WriteLine("Server port when server initialized:" + endURL[0]);
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = Int32.Parse(endURL[0]);
            this.channel = new TcpChannel(props, null, provider);
            // this.channel = new TcpChannel(Int32.Parse(endURL[0]));
            //ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(serverObj, serverID, typeof(ServerServices));
        }


        private void delayRandomTime()
        {
            Thread.Sleep(millSecWait);
        }

        public void PrintStatus()
        {
            Console.WriteLine("I am " + serverID);
            Console.WriteLine("URL: " + myServerURL);
            Console.WriteLine("Max faults: " + max_faults);

            string otherServers = "Other servers: ";
            foreach (string url in otherServerURLs)
            {
                otherServers += url + ", ";
            }
            Console.WriteLine(otherServers);

            string clientsStr = "Clients: ";
            foreach(KeyValuePair<string, IClientServices> entry in this.clients)
            {
                clientsStr += entry.Key + ", ";
            }
            Console.WriteLine(clients);

            string meetingTopics = "Meetings: ";
            foreach (IMeetingServices meeting in this.meetings)
            {
                otherServers += meeting.getTopic() + ", ";
            }
            Console.WriteLine(meetingTopics);
        }

        public List<IMeetingServices> getMeetings()
        {
            return this.meetings;
        }
        public List<IServerServices> Servers 
        { 
            get { return otherServers; }  
        }
        public string getServerURL()
        {
            return this.myServerURL;
        }

        public string getServerID()
        {
            delayRandomTime();
            return this.serverID;
        }

        public List<string> getOtherServerURLs()
        {
            return this.otherServerURLs;
        }
        public int getMaxFaults()
        {
            return this.max_faults;
        }

        private void setAllOtherServers(string otherServerURL)
        {
            if (this.myServerURL != otherServerURL)
            {
                this.AddNewServer(otherServerURL);
            }
            IServerServices serverFromURL = (IServerServices)Activator.GetObject(typeof(IServerServices),
                otherServerURL);
            serverFromURL.AddNewServer(this.myServerURL);
            try
            {
                foreach (string serverURL in serverFromURL.getOtherServerURLs())
                {
                    if (serverURL != null && !otherServerURLs.Contains(serverURL))
                    {
                        if (this.myServerURL != serverURL)
                        {
                            this.AddNewServer(serverURL);
                        }
                        
                        IServerServices server = (IServerServices)Activator.GetObject(typeof(IServerServices),
                        serverURL);
                        server.AddNewServer(this.myServerURL); //adds this new server to the remote server
                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        public void AddNewServer(string serverURL)
        {
            otherServerURLs.Add(serverURL);
            IServerServices server = (IServerServices)Activator.GetObject(typeof(IServerServices), serverURL);
            otherServers.Add(server);
            
        }
        public void setSequencer(string sequencerURL)
        {
            if (sequencerURL == myServerURL)
            {
                isSequencer = true;
                sequencer = null;
                seqNr = 0;
            }
            else
            { 
             IServerServices sequencerServer = (IServerServices)Activator.GetObject(typeof(IServerServices),
                           sequencerURL);
            sequencer = sequencerServer;
            isSequencer = false;
            }
         
        }

        public Boolean closeMeetingProposal(string meetingTopic, string coordinatorUsername)
        {
            bool foundMeeting = false;
            bool foundBestDateAndLocation = false;
            //One server solution
            foreach (MeetingServices meeting in this.meetings)
            {

                if (meeting.Topic == meetingTopic)
                {
                    foundBestDateAndLocation = this.findBestDateAndLocation(meeting);
                    foundMeeting = true;
                }
                
            }
            //checks for meeting in other servers if meeting not in this server (multiple servers solution)
            if (!foundMeeting)
            {
                foreach (IServerServices server in otherServers)
                {
                    foreach (MeetingServices meeting in server.getMeetings())
                    {
                        if (meeting.Topic == meetingTopic) //finds the unique meeting
                        {
                            foundBestDateAndLocation = this.findBestDateAndLocation(meeting);
                            foundMeeting = true;
                        }
                    }
                }
            }
            if (!foundMeeting || !foundBestDateAndLocation)
            {
                return false; //could not find unique meeting or it did not exist a date and location that fitted
            }
            return true; //closed meeting

        }

        private bool findBestDateAndLocation(MeetingServices meeting)
        {
            int maxNumParticipants = 0;
            (string, DateTime) bestLocAndDate = meeting.LocDateOptions[0];
            Room bestroom = new Room("default", 0);
            foreach ((string, DateTime) locdateoption in meeting.LocDateOptions)
            {
                List<Room> availableRooms = new List<Room>();
                // checks if location has available rooms and stores them
                foreach (Room room in location.GetRooms(locdateoption.Item1))
                {
                    //check if room is booked on date requested for meeting and if participants dont exceed capacity of room
                    if (!room.BookedDates.Contains(locdateoption.Item2) && meeting.MinParticipants <= room.Capacity) 
                    {
                        availableRooms.Add(room);
                    }
                }
                int numParticipants = 0;
                if (meeting.Participants.Keys.Contains(locdateoption))
                {
                    numParticipants = meeting.Participants[locdateoption].Count;
                }
                if (numParticipants > maxNumParticipants) //if it has more participants then the previous locAndDate
                {
                    maxNumParticipants = numParticipants;
                    bestLocAndDate = locdateoption;
                    bestroom = this.getSmallestRoom(availableRooms, numParticipants); //set the best room to 
                    //the smallest one of the available rooms in this best locDate
                }
            }
            if (bestroom.Capacity < maxNumParticipants)
            {
                int numPeopleToExclude = maxNumParticipants - bestroom.Capacity;
                //exclude the last people that entered the participants list
                for (int i = meeting.Participants[bestLocAndDate].Count - 1; i >= meeting.Participants[bestLocAndDate].Count-numPeopleToExclude; i--)
                {
                    meeting.Participants[bestLocAndDate].RemoveAt(i);
                }
                
            }
            bestroom.BookedDates.Add(bestLocAndDate.Item2); //books room for the date in bestLocAndDate
            meeting.Closed = true; 
            //this.meetings.Remove(meeting); Teacher said: "do not remove meeting after close"
            return true;
        }

        
        private Room getSmallestRoom(List<Room> availableRooms, int numParticipants)
        {
            IEnumerable<Room> ascendingRoomsByCapacity = availableRooms.OrderBy(room => room.Capacity);
            foreach (Room room in ascendingRoomsByCapacity)
            {
                if (room.Capacity >= numParticipants)
                {
                    return room;
                }
            }
            //returns the room with the largest capacity if more participants than capacity
            return ascendingRoomsByCapacity.Last(); 
        }

        public void NewMeetingProposal(IMeetingServices proposal)
        {
            if (!meetings.Contains(proposal))
            {
                meetings.Add(proposal);
            }
        }

        public List<int> distributeMeetingsToFOtherServers()
        {
            List<int> crashedServerIndexes = new List<int>();
            for (int i = 0; i < max_faults; i++)
            {
                int serverIndex = rnd.Next(0, otherServers.Count);
                foreach (IMeetingServices meeting in meetings)
                {
                    try
                    {
                        otherServers[serverIndex].NewMeetingProposal(meeting);
                    }
                    catch (Exception e)
                    {
                        crashedServerIndexes.Add(serverIndex);
                    }
                    
                }
                
            }
            return crashedServerIndexes;
        }

        public void notifyOtherServersToDistributeMeetings(List<int> crashedServerIndexes)
        {
            foreach (int serverIndex in crashedServerIndexes)
            {
                Servers.RemoveAt(serverIndex);
            }
            foreach (ServerServices server in Servers)
            {
                server.distributeMeetingsToFOtherServers();
            }
        }

        public void NewClient(string uname, string userURL)
        {
            lock (clients)
            {
                if (!clients.ContainsKey(uname))
                {
                    try
                    {
                        IClientServices cli = (IClientServices)Activator.GetObject(typeof(IClientServices),
                    userURL);
                        clients.Add(uname, cli);
                        clientURLs.Add(userURL);
                    }catch (Exception e)
                    {
                        Console.WriteLine("HELLLOOOOOO" + e);
                    }  
                }
            }
        }
    
        public List<string> getSampleClientsFromOtherServers()
        {
            List<string> samples = new List<string>();
         

            foreach (ServerServices server in otherServers)
            {
                string clientURL = server.getRandomClientURL();
                Console.WriteLine(clientURL);
                if (clientURL != null)
                {
                    samples.Add(clientURL);
                }
                Console.WriteLine("[from getSampleCLientsFromOtherServers] Random clientURL:  " + clientURL + "   from server: " + server.getServerURL());             
            }
            return samples;
        }
   
        public string getRandomClientURL()
        {
            if (clients.Count == 0)
            {
                return null;
            }
            Random r = new Random();
            int randomIndex = r.Next(0, clients.Count);
            return clientURLs[randomIndex];
        }

        public List<string> getOwnClients() {

            Console.WriteLine("I am server " + this.serverID + " and my clients are: ");
            foreach (string URL in clientURLs)
            {
                Console.WriteLine(URL);
            }
            return clientURLs;
        }
        public void AddRoom(string location, int capacity, string roomName)
        {
            Room newRoom = new Room(roomName, capacity);
            this.location.addRoom(newRoom, location);
        }
          

        public void JoinMeeting(string meetingTopic, string userName,
            bool requesterIsClient, List<(string, DateTime)> dateLoc)
        {
            foreach (MeetingServices meeting in meetings)
            {
                if (meeting.Topic == meetingTopic && meeting.IsInvited(userName))
                {
                    meeting.JoinMeeting(userName, dateLoc);
                    break;
                }
            }
            if (requesterIsClient)
            {
                foreach (IServerServices meetingServer in otherServers)
                {
                    meetingServer.JoinMeeting(meetingTopic, userName, false, dateLoc);
                }
            }
        }

        public List<IMeetingServices> ListMeetings(string userName, List<IMeetingServices> meetingClientKnows, bool requesterIsClient)
        {
            List<IMeetingServices> availableMeetings = new List<IMeetingServices>();
            IEnumerable<IMeetingServices> intersectMeetings = meetings.Intersect(meetingClientKnows);
            foreach (MeetingServices meeting in intersectMeetings)
            {
                if (meeting.IsInvited(userName) )
                {
                    availableMeetings.Add(meeting);
                }
            }
            if (requesterIsClient)
            {
                foreach (IServerServices server in otherServers)
                {
                    foreach (IMeetingServices meets in server.ListMeetings(userName, meetingClientKnows, false))
                    {
                        availableMeetings.Add(meets);
                    }
                }
            }
            return availableMeetings;
        }

        public void serverKill()
        {
            Process[] runningProcesses = Process.GetProcessesByName("Server");
            foreach (Process process in runningProcesses)
            {
                process.Kill();
                process.WaitForExit();
                //foreach (ProcessModule module in process.Modules)
                //{
                //    if (module.FileName.Equals("Server.exe"))
                //    {
                //        process.Kill();
                //    }
                //}
            }

        }

        public void freeze()
        {
            if (this.frozenMode == false)
            {
                this.frozenMode = true;
            }  
        }

        public void unfreeze()
        {
            if (this.frozenMode == true)
            {
                this.frozenMode = false;
            }
        }

        public override bool Equals(object obj) => Equals(obj as ServerServices);
        public override int GetHashCode() => (serverID).GetHashCode();

        public bool Eqauls(ServerServices server)
        {
            if (server is null) return false;
            return this.serverID == ((ServerServices)server).getServerID();
        }
        public bool Equals(ServerServices other)
        {
            if (other is null) return false;
            return this.serverID == other.getServerID();
        }
       
        public void InformSeqFailed()
        {
            foreach (IServerServices server in otherServers)
            {
                try
                {
                    server.electNewSequencer(sequencer);
                }
                catch (Exception e)
                {
                    RemoveFailedServer(server);
                    Console.WriteLine("Did not succeed in informing about sequencer failed:  " + e);
                }  
            }
        }

        public void electNewSequencer(IServerServices failedSequencer)
        {
            if (failedSequencer.Equals(sequencer)) { 
                 //be sure list of other servers is up to date
                   RemoveFailedServer(failedSequencer);
                   sequencer = otherServers[0];
                if (sequencer.Equals(this))
                 {
                prepareToBeSequencer();
                  }
            }  
        }

        private void prepareToBeSequencer()
        {
            isSequencer = true;
            int maxSeqNr = 0;
            foreach (IServerServices server in otherServers)
            {
                try
                {
                     int tempMaxSeqNr = server.getHighestSeqNr();
                    if (tempMaxSeqNr > maxSeqNr)
                    {
                        maxSeqNr = tempMaxSeqNr;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Server did not respond when getting highestSeqNr. Error message:  "+ e);
                    RemoveFailedServer(server);
                }
            }
            seqNr = maxSeqNr;
        }

        public int getHighestSeqNr()
        {
            List<int> pendingTasks = new List<int>();
            int maxSeqNr = pendingTasks.Max();
            return maxSeqNr;
        }

        // 0 is invalid seqnr
        public int handOutSeqNumber()
        {
            if (isSequencer)
            {
                seqNr++;
                return seqNr;
            }
            else { return 0; }
        }

        //Should removeFailedServer be highest priority?
        //possible to use both server and serverURL to remove from both lists
        private void RemoveFailedServer(IServerServices failedServer = null, string failedServerURL = null) 
        {
            if (failedServer != null)
            {
                if (otherServers.Contains(failedServer))
                {
                    int index = otherServers.IndexOf(failedServer); 
                    otherServers.RemoveAt(index);
                    otherServerURLs.RemoveAt(index);
                }
            }
            else if (failedServerURL != null)
            {
                if (otherServerURLs.Contains(failedServerURL)){
                    int index = otherServerURLs.IndexOf(failedServerURL);
                    otherServers.RemoveAt(index);
                    otherServerURLs.RemoveAt(index);
                }
            }
        }

        static void Main(string[] args)
        {
            
            ServerServices server = new ServerServices(args[0], args[1], args[2], Int32.Parse(args[3]),
                Int32.Parse(args[4]), Int32.Parse(args[5]));
            server.initialize(args[2], args[1], server);
            
            Console.WriteLine("<Enter> to exit...");
            Console.ReadLine();
        }

    }
}
