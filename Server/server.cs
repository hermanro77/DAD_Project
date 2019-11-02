using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace MeetingCalendar
{
    public class ServerServices : MarshalByRefObject, IServerServices
    {
        private Dictionary<string, IClientServices> clients = new Dictionary<string, IClientServices>();
        private List<IServerServices> servers;
        private List<IMeetingServices> meetings;
        private Location location = new Location();

        public ServerServices(Dictionary<string, IClientServices> clients, List<IServerServices> servers)
        {
            this.clients = clients;
            this.servers = servers;
        }

        public Boolean closeMeetingProposal(string meetingTopic, string coordinatorUsername)
        {
            Boolean foundMeeting = false;
            Boolean foundBestDateAndLocation = false;
            foreach (MeetingServices meeting in this.meetings)
            {

                if (meeting.Topic == meetingTopic)
                {
                    foundBestDateAndLocation = this.findBestDateAndLocation(meeting);
                    foundMeeting = true;
                }
                
            }
            //checks for meeting in other services
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

        private Boolean findBestDateAndLocation(MeetingServices meeting)
        {
            int maxNumParticipants = 0;
            (string, DateTime) bestLocAndDate = meeting.LocDateOptions[0];
            Room bestroom = null;
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
                    bestroom = this.getSmallestRoom(availableRooms); //set the best room to 
                    //the smallest one of the available rooms in this best locDate
                }
            }
            
            bestroom.BookedDates.Add(bestLocAndDate.Item2); //books room for the date in bestLocAndDate
            meeting.Closed = true; 
            this.meetings.Remove(meeting);
            return true;
        }

        private Room getSmallestRoom(List<Room> availableRooms)
        {
            Room smallestRoom = availableRooms[0];
            foreach (Room room in availableRooms)
            {
                if (room.Capacity < smallestRoom.Capacity)
                {
                    smallestRoom = room;
                }
            }
            return smallestRoom;
        }

        public void NewMeetingProposal(IMeetingServices proposal)
        {
            meetings.Add(proposal);
        }

        public void NewUser(string uname, int port)
        {
            if (!clients.ContainsKey(uname))
            {
                IClientServices cli = (IClientServices)Activator.GetObject(typeof(IClientServices),
                "tcp://localhost:" + port + "/MyRemoteClient");
                clients.Add(uname, cli);
            }
            // throw new NotImplementedException();
        }
    }
    class Server
    {
        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(8086);

            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(IServerServices),
                "MyRemoteServer",
                WellKnownObjectMode.Singleton);

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }

        public static void HostNewMeeting(string uid)
        {
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(IMeetingServices),
                "RemoteMeeting" + uid,
                WellKnownObjectMode.Singleton);
        }
    }
}
