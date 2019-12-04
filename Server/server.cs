using CommonTypes;
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
        private List<IServerServices> allServers = new List<IServerServices>();
        private List<string> allServerURLs = new List<string>();
        private List<string> clientURLs = new List<string>();
        private List<IMeetingServices> meetings = new List<IMeetingServices>();
        private Location location = new Location();
        private int millSecWait;
        private string myServerURL
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
                AddNewServer(myServerURL); //add myself to lists
                sequencer = allServers[0];
                isSequencer = false;
            }
            else
            {
                //sets sequencer to be myself
                AddNewServer(myServerURL); //add myself to lists
                sequencer = allServers[0];
                isSequencer = true;
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
            foreach (string url in allServerURLs)
            {
                if (url != myServerURL) { otherServers += url + ", ";}
                
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
            List<string> serversWithoutMe = this.allServerURLs;
            serversWithoutMe.Remove(myServerURL);
            return serversWithoutMe;
        }
        public List<IServerServices> getServers()
        {
            return allServers;
        }
        public int getMaxFaults()
        {
            return this.max_faults;
        }

        private void setAllOtherServers(string otherServerURL)
        {
            IServerServices serverFromURL = (IServerServices)Activator.GetObject(typeof(IServerServices),
                otherServerURL);
           
            try
            {
                foreach (string serverURL in serverFromURL.getOtherServerURLs())
                {
                    if (serverURL != null && !allServerURLs.Contains(serverURL))
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
             serverFromURL.AddNewServer(this.myServerURL);
        }

        public void AddNewServer(string serverURL)
        {
            allServerURLs.Add(serverURL);
            IServerServices server = (IServerServices)Activator.GetObject(typeof(IServerServices), serverURL);
            allServers.Add(server);  
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
                foreach (IServerServices server in getServers())
                {
                    if (server.getServerURL() != this.myServerURL)
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

        public void failedServerDetected(string serverURL)
        {
            RemoveFailedServer(failedServerURL: serverURL);
            notifyServersToDistributeMeetings(); 
            if (sequencer.getServerURL() == serverURL)
            {
                InformSeqFailed();
            }
        }

        public void distributeMeetingsToFOtherServers()
        {
             foreach (IMeetingServices meeting in meetings){
                int loopCount = 0;
                int index = rnd.Next(0, allServers.Count-1);
                int i = 0;
                while (i < max_faults)
                {
                    IServerServices serverToGetMeeting = allServers[(index +loopCount)  % allServers.Count]; //mod to not get index out of bound
                    try
                    {
                        if (serverToGetMeeting.getServerURL() != this.myServerURL) { 
                            serverToGetMeeting.NewMeetingProposal(meeting);
                            i++;
                        } 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Server" + serverID + "detected a failed server.");
                        RemoveFailedServer(serverToGetMeeting);
                    }
                    loopCount++;
                    if (loopCount == allServers.Count) //have now tried every server
                    {
                        Console.WriteLine("There are less servers than the max faults. There is a probability that the meeting is not replicated on enough servers.");
                        break;
                    }
                }
             }
        }

        private void notifyServersToDistributeMeetings()
        {
            foreach(IServerServices server in allServers) //her vil også serveren selv bli kalt, usikker på hvordan det vil fungere.
            {
                try
                {
                    server.distributeMeetingsToFOtherServers();
                } catch (Exception e)
                {
                    Console.WriteLine("Server" + serverID + "detected a failed server.");
                    if (server.Equals(sequencer))
                    {
                        InformSeqFailed();
                    }
                    else { 
                        RemoveFailedServer(server);  
                    }
                    
                }
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
            Console.WriteLine("Hi from " + serverID+ ". Giving client");

            foreach (ServerServices server in allServers)
            {
                if (server.getServerURL() != this.myServerURL)
                {
                    string clientURL = server.getRandomClientURL();
                    Console.WriteLine(clientURL);
                    if (clientURL != null)
                    {
                        samples.Add(clientURL);
                    }
                    Console.WriteLine("[from getSampleCLientsFromOtherServers] Random clientURL:  " + clientURL + "   from server: " + server.getServerURL());
                }
            }
            if (samples.Count > 0)
            {
                return samples;
            }
           
            return null;
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
                foreach (IServerServices meetingServer in allServers)
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
                foreach (IServerServices server in allServers)
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
            foreach (IServerServices server in allServers)
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
                   RemoveFailedServer(failedSequencer);
                   sequencer = allServers[0];
                if (sequencer.getServerURL() == this.myServerURL)
                 {
                prepareToBeSequencer();
                  }
            }  
        }

        private void prepareToBeSequencer()
        {
            isSequencer = true;
            int maxSeqNr = 0;
            foreach (IServerServices server in allServers)
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
            //TODOO: refer to rigth pendingTasks
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
                if (allServers.Contains(failedServer))
                {
                    int index = allServers.IndexOf(failedServer); 
                    allServers.RemoveAt(index);
                    allServerURLs.RemoveAt(index);
                }
            }
            else if (failedServerURL != null)
            {
                if (allServerURLs.Contains(failedServerURL)){
                    int index = allServerURLs.IndexOf(failedServerURL);
                    allServers.RemoveAt(index);
                    allServerURLs.RemoveAt(index);
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
