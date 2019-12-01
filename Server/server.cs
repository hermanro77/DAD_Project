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
using System.Threading.Tasks;
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
        private string serverURL;
        private string serverID;
        private int max_faults;
        private bool frozenMode = false;
        List<Task> pendingTasks = new List<Task>();

        TcpChannel channel;
        private Random rnd = new Random();

        // serverURLs is a list of tuples on the form (Server_URL, Serve_ID) for the other servers to communicate with
        public ServerServices(string otherServerURL, string serverID, string serverURL, int max_faults,
            int minWait, int maxWait)
        {
            this.serverID = serverID;
            this.millSecWait = (minWait == 0 && maxWait == 0) ? 0 : rnd.Next(minWait, maxWait);
            this.serverURL = serverURL;
            this.max_faults = max_faults;
            if (otherServerURL != null && otherServerURL.Contains("tcp")) //if it's not the first server created find all other servers in system
            {
                setAllOtherServers(otherServerURL); //uses otherServerURL to get all servers currently set up and add them to serverURLs. 
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
            Console.WriteLine("URL: " + serverURL);
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
            return this.serverURL;
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
            if (this.getServerURL() != otherServerURL)
            {
                this.AddNewServer(otherServerURL);
            }
            IServerServices serverFromURL = (IServerServices)Activator.GetObject(typeof(IServerServices),
                otherServerURL);
            
            try
            {
                foreach (string serverURL in serverFromURL.getOtherServerURLs())
                {
                    if (serverURL != null && !otherServerURLs.Contains(serverURL))
                    {
                        if (this.getServerURL() != serverURL)
                        {
                            this.AddNewServer(serverURL);
                        }
                        
                        IServerServices server = (IServerServices)Activator.GetObject(typeof(IServerServices),
                        serverURL);
                        server.AddNewServer(this.getServerURL()); //adds this new server to the remote server
                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
            serverFromURL.AddNewServer(this.getServerURL());
        }
        public void AddNewServer(string serverURL)
        {
            otherServerURLs.Add(serverURL);
            IServerServices server = (IServerServices)Activator.GetObject(typeof(IServerServices), serverURL);
            otherServers.Add(server);
        }

        public bool closeMeetingProposal(string meetingTopic, string coordinatorUsername, bool requestCommingFromClient)
        {
            
            if (frozenMode || (!frozenMode && pendingTasks.Count > 0 && requestCommingFromClient)) // if frozen or close request is made while the server is executing the tasks in his pending list
            {
                Console.WriteLine("Added task to pending list");
                Task<bool> newTask = new Task<bool>(() => closeMeetingProposal(meetingTopic, coordinatorUsername, false));
                pendingTasks.Add(newTask);
                return false;
            }
            
            else 
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

        public void NewMeetingProposal(IMeetingServices proposal, bool requestCommingFromClient)
        {
            if (frozenMode || (!frozenMode && pendingTasks.Count > 0 && requestCommingFromClient)) // if frozen or close request is made while the server is executing the tasks in his pending list
            {
                Console.WriteLine("Added task to pending list");
                Task newTask = new Task(() => NewMeetingProposal(proposal, false));
                pendingTasks.Add(newTask);
            }

            else
            {
                if (!meetings.Contains(proposal))
                {
                    meetings.Add(proposal);
                }
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
                        otherServers[serverIndex].NewMeetingProposal(meeting, true);
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
                    }
                    catch (Exception e)
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
            if (frozenMode || (!frozenMode && pendingTasks.Count > 0 && requesterIsClient)) // if frozen or close request is made while the server is executing the tasks in his pending list
            {
                Console.WriteLine("Added task to pending list");
                Task newTask = new Task(() => JoinMeeting(meetingTopic, userName, false, dateLoc));
                pendingTasks.Add(newTask);
            }
            else
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
                process.WaitForExit(); // vil vi dette?
            }

        }

        public void freeze()
        {
            if (this.frozenMode == false)
            {
                Console.WriteLine("turning on freeze mode");
                this.frozenMode = true;
            }
            
        }

        public void unfreeze()
        {
            if (this.frozenMode == true)
            {
                Console.WriteLine("turning of freeze mode");
                this.frozenMode = false;

                int tasksRemaining = pendingTasks.Count;
                while (tasksRemaining > 0)
                {
                    Console.WriteLine("Entered while");
                    for (int i = 0; i < tasksRemaining; i++)
                    {
                        Thread.Sleep(10000);
                        Console.WriteLine("executing task number " + i + " in pending");
                        Task task = pendingTasks[i];
                        task.Start();
                    }
                    pendingTasks.RemoveRange(0, tasksRemaining);
                    tasksRemaining = pendingTasks.Count;
                    Console.WriteLine("Tasks remaining: " + tasksRemaining);
                }
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
