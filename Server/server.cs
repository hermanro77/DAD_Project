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
        private List<string> servers;
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

        private void findBestDateAndLocation(MeetingServices meeting)
        {
            return;
        }

        public void NewMeetingProposal(MeetingServices proposal)
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
