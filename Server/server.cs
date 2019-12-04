using CommonTypes;
using Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using static CommonTypes.CommonType;

namespace MeetingCalendar
{
    public class ServerServices : MarshalByRefObject, IServerServices
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
        private int sqNum = 0;

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
            if (otherServerURL.Contains("tcp")) //if it's not the first server created find all other servers in system
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


        public void PrintStatus(int sequenceNumber = -1)
        {
            if (sequenceNumber == -1)
            {
                sequenceNumber = this.sequenceServer.getSeqNum();
            }
            if (sequenceNumber <= this.sqNum)
            {
                return;
            } 
            else if (sequenceNumber == this.sqNum + 1)
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
                foreach (KeyValuePair<string, IClientServices> entry in this.clients)
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
                this.sqNum += 1;
                this.informServersNewSeq(sequenceNumber);
            } 
            else
            {
                // SET THIS TASK IN PENDING
            }
            
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

        public Boolean closeMeetingProposal(string meetingTopic, string coordinatorUsername, int sequenceNumber = -1)
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
            // Distribute the meeting to f other servers
            meetings.Add(proposal);
        }

        public void updateSequence(int sequenceNumber)
        {
            if (sequenceNumber == this.sqNum + 1)
            {
                this.sqNum += 1;
            } else if (sequenceNumber > this.sqNum)
            {
                // SET IN PENDING LIST
            }
        }

        private void informServersNewSeq(int sequenceNumber)
        {
            foreach (IServerServices servers in otherServers)
            {
                try
                {
                    servers.updateSequence(sequenceNumber);
                }
                catch
                {
                    // CALL FUNCTION THAT REMOVES THE CRASHED SERVER FROM OUR LIST
                }
            }
        }

        public void NewClient(string uname, string userURL)
        {
            int seqNum = this.sequenceServer.getSeqNum();
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
            this.informServersNewSeq(seqNum);
        }
    
        public List<string> getSampleClientsFromOtherServers(int sequenceNumber = -1)
        {
            List<string> samples = new List<string>();
            if (sequenceNumber == -1)
            {
                sequenceNumber = this.sequenceServer.getSeqNum();
            }
            foreach (ServerServices server in otherServers)
            {
                string clientURL = server.getRandomClientURL(sequenceNumber);
                Console.WriteLine(clientURL);
                if (clientURL != null)
                {
                    samples.Add(clientURL);
                }
                //Console.WriteLine("[from getSampleCLientsFromOtherServers] Random clientURL:  " + clientURL + "   from server: " + server.getServerURL(sequenceNumber));
            }
            if (sequenceNumber == this.sqNum)
            {
                return new List<string>();
            }
            else if (sequenceNumber == this.sqNum + 1)
            {
                return samples;
            }
            else
            {
                // SET THIS TASK AS PENDING
            }
           
        }
   
        public string getRandomClientURL(int sequenceNumber = -1)
        {
            if (sequenceNumber == -1)
            {
                sequenceNumber = this.sequenceServer.getSeqNum();
            }
            if (sequenceNumber == this.sqNum)
            {
                return "";
            }
            else if (sequenceNumber == this.sqNum + 1)
            {
                if (clients.Count == 0)
                {
                    return null;
                }
                int randomIndex = this.rnd.Next(0, clients.Count);
                return clientURLs[randomIndex];
            }
            else
            {
                // SET THIS TASK AS PENDING
                return null;
            }
            
        }

        public List<string> getOwnClients(int sequenceNumber = -1) {
            if (sequenceNumber == -1)
            {
                sequenceNumber = this.sequenceServer.getSeqNum();
            }
            if (sequenceNumber == this.sqNum)
            {
                return new List<string>();
            }
            else if (sequenceNumber == this.sqNum + 1)
            {
                Console.WriteLine("I am server " + this.serverID + " and my clients are: ");
                foreach (string URL in clientURLs)
                {
                    Console.WriteLine(URL);
                }
                this.sqNum += 1;
                return clientURLs;
            }
            else
            {
                // SET THIS TASK AS PENDING
            }
            
        }
        public void AddRoom(string location, int capacity, string roomName, int sequenceNumber = -1)
        {
            if (sequenceNumber == -1)
            {
                sequenceNumber = this.sequenceServer.getSeqNum();
            }
            else if (sequenceNumber == this.sqNum + 1)
            {
                Room newRoom = new Room(roomName, capacity);
                this.location.addRoom(newRoom, location);
                this.sqNum += 1;
            }
            else
            {
                // SET THIS ACTIVITY AS PENDING
            }
            
        }
          

        public void JoinMeeting(string meetingTopic, string userName, List<(string, DateTime)> dateLoc, int sequenceNumber = -1)
        {
            if (sequenceNumber == -1)
            {
                sequenceNumber = this.sequenceServer.getSeqNum();
            }
            foreach (IServerServices meetingServer in otherServers)
            {
                meetingServer.JoinMeeting(meetingTopic, userName, dateLoc, sequenceNumber);
            }
            if (sequenceNumber <= this.sqNum)
            {
                return;
            }
            else if (sequenceNumber == this.sqNum+1)
            {
                foreach (MeetingServices meeting in meetings)
                {
                    if (meeting.Topic == meetingTopic && meeting.IsInvited(userName))
                    {
                        meeting.JoinMeeting(userName, dateLoc);
                        break;
                    }
                }
                
                this.sqNum += 1;
            }
            else
            {
                // SET THIS AS PENDING
            }
            
        }

        public void CloseMeetingProposal(string meetingTopic, string coordinatorUsername)
        {
            throw new NotImplementedException();
        }

        public List<IMeetingServices> ListMeetings(string userName, List<IMeetingServices> meetingClientKnows, int sequenceNumber = -1)
        {
            List<IMeetingServices> availableMeetings = new List<IMeetingServices>();
            if (sequenceNumber == -1)
            {
                sequenceNumber = this.sequenceServer.getSeqNum();
            }

            foreach (IServerServices server in otherServers)
            {
                foreach (IMeetingServices meets in server.ListMeetings(userName, meetingClientKnows, sequenceNumber))
                {
                    availableMeetings.Add(meets);
                }
            }

            if (sequenceNumber <= this.sqNum)
            {
                return new List<IMeetingServices>();
            } 
            else if (sequenceNumber == this.sqNum +1)
            {
                IEnumerable<IMeetingServices> intersectMeetings = meetings.Intersect(meetingClientKnows);
                foreach (MeetingServices meeting in intersectMeetings)
                {
                    if (meeting.IsInvited(userName))
                    {
                        availableMeetings.Add(meeting);
                    }
                }
                this.sqNum += 1;
                return availableMeetings;
            }  else
            {
                // SET THIS AS A PENDING TASK!!
                return null;
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

        public List<string> getClients()
        {
            throw new NotImplementedException();
        }
    }
}
