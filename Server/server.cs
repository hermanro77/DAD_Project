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

        public void closeMeetingProposal(string meetingTopic, string coordinatorUsername)
        {
            foreach (MeetingServices meeting in meetings)
            {
                if (meeting.Topic == meetingTopic)
                {
                    this.findBestDateAndLocation(meeting);
                    meeting.Closed = true;
                }
                else
                {

                }
            }
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

        private void findBestDateAndLocation(MeetingServices meeting)
        {
            return;
        }

        public void NewMeetingProposal(MeetingServices proposal)
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
