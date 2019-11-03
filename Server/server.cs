using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using static CommonTypes.CommonType;

namespace MeetingCalendar
{
    public class ServerServices : MarshalByRefObject, IServerServices
    {
        private Dictionary<string, IClientServices> clients = new Dictionary<string, IClientServices>();
        private List<IServerServices> servers;
        private List<string> serverURLs;
        private List<IMeetingServices> meetings;
        private Location location = new Location();

        public ServerServices(Dictionary<string, IClientServices> clients, List<IServerServices> servers)
        {
            this.clients = clients;
            this.servers = servers;
        }

        public bool closeMeetingProposal(string meetingTopic, string coordinatorUsername)
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
            //checks for meeting in other services if meeting not in server (multiple servers solution)
            if (!foundMeeting)
            {
                foreach (ServerServices server in servers)
                {
                    foreach (MeetingServices meeting in server.meetings)
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
                return false; //could not find meeting or it did not exist a date and location that fitted
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
                foreach (Room room in location.GetRooms[locdateoption.Item1])
                {
                    //check if room is booked on date requested for meeting and if participants dont exceed capacity of room
                    if (!room.BookedDates.Contains(locdateoption.Item2) && meeting.MinParticipants <= room.Capacity) 
                    {
                        availableRooms.Add(room);
                    }
                }
                int numParticipants = meeting.Participants[locdateoption].Count;
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
            //this.meetings.Remove(meeting); Remove meeting after close?
            return true;
        }

        // serverURLs is a list of tuples on the form (Server_URL, Serve_ID) for the other servers to communicate with
        public ServerServices(List<string> serverURLs, string serverID, string serverURL)
        {
            this.serverURLs = serverURLs;
            this.SetupServers();
            string[] partlyURL = serverURL.Split(':');
            string[] endURL = partlyURL[partlyURL.Length - 1].Split('/');
            TcpChannel channel = new TcpChannel(Int32.Parse(endURL[0]));

            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerServices),
                serverID,
                WellKnownObjectMode.Singleton);
        }

        private void SetupServers()
        {
            foreach (string url in serverURLs)
            {
                IServerServices server = (IServerServices)Activator.GetObject(typeof(IServerServices),
                url);
                servers.Add(server);
            }
        }

        public void AddNewServer(string serverURL)
        {
            IServerServices server = (IServerServices)Activator.GetObject(typeof(IServerServices), serverURL);
            servers.Add(server);
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
            meetings.Add(proposal);
        }

        public void NewUser(string uname, string userURL)
        {
            if (!clients.ContainsKey(uname))
            {
                IClientServices cli = (IClientServices)Activator.GetObject(typeof(IClientServices),
                userURL);
                clients.Add(uname, cli);
            }
            // throw new NotImplementedException();
        }

        public void NewMeetingProposal(IMeetingServices proposal)
        {
            throw new NotImplementedException();
        }

        public void JoinMeeting(string meetingTopic, string userName,
            bool requesterIsClient, List<(string, DateTime)> dateLoc)
        { 
            foreach (MeetingServices meeting in meetings)
            {
                if (meeting.Topic == meetingTopic)
                {
                    meeting.JoinMeeting(userName);
                    break;
                }
            }
            if (requesterIsClient)
            {
                foreach (ServerServices meetingServer in servers)
                {
                    meetingServer.JoinMeeting(meetingTopic, userName, false, dateLoc);
                }
            }
        }

        public void CloseMeetingProposal(string meetingTopic, string coordinatorUsername)
        {
            throw new NotImplementedException();
        }

        public List<IMeetingServices> ListMeetings(string userName, bool requesterIsClient)
        {
            List<IMeetingServices> availableMeetings = new List<IMeetingServices>();
            foreach (MeetingServices meeting in meetings)
            {
                if (meeting.IsInvited(userName))
                {
                    availableMeetings.Add(meeting);
                }
            }
            if (requesterIsClient)
            {
                foreach (IServerServices server in servers)
                {
                    foreach(MeetingServices meets in server.ListMeetings(userName, false))
                    {
                        availableMeetings.Add(meets);
                    }

                }
            }
            return availableMeetings;
        }
    }
}
