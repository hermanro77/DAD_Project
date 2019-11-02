using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace Client
{
    class Client
    {
        
        private List<string> myCreatedMeetings = new List<string>();
        private string userName;
        IServerServices myServer;
        static void Main(string[] args)
        {
        
        }

        public Client(string userName, string clientURL, string serverURL, string scriptFileName)
        {
            this.userName = userName;
            this.setUp(clientURL, serverURL);
        }

        private void runScript()
        {
            string[] command;
            StreamReader script = new StreamReader(@"PATH");
            while((command = script.ReadLine().Split(',')) != null)
            {
                switch (command[0])
                {
                    case "list":
                        this.listMeetings();
                        break;
                    case "create":
                        this.createMeeting(command[1], Int32.Parse(command[2]),
                            this.ParseDateLoc(command), this.ListInvitees(command));
                        break;
                    case "join":
                        this.joinMeeting(command[1]);
                        break;
                    case "close":
                        this.closeMeeting(command[1]);
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

        private List<(string, DateTime)> ParseDateLoc(string[] entries)
        {
            List<(string, DateTime)> slots = new List<(string, DateTime)>();
            int n = Int32.Parse(entries[3]);
            for (int i = 5; i < 5 + n; i++)
            {
                string[] dateLoc = entries[i].Replace(@"(", "").Replace(@")", "").Split(';');
                slots.Add((dateLoc[0], DateTime.Parse(dateLoc[1])));
            }
            return slots;
        }

        private void wait(int delayTime)
        {
            // Not sure if works, temporary solution
            System.Threading.Thread.Sleep(delayTime);
        }
        private void createMeeting(string meetingTopic, int minAttendees, int numSlots, int numInvites,
            List<(string, DateTime)> slots, List<string> invitees)
        {
            // Create new meeting
            // USE TRY-CATCH
        }

        private void joinMeeting(string meetingTopic)
        {
            // Joins an existing meeting, using meetingTopic as unique ID
            // USE TRY-CATCH
        }

        private void closeMeeting(string meetingTopic)
        {
            if (myCreatedMeetings.Contains(meetingTopic))
            {
                try
                {
                    myServer.closeMeeting(meetingTopic);
                } catch (Exception e)
                {
                    changeServer();
                }
            }
        }

        private void listMeetings()
        {
            myServer.getAvailableMeetings(userName);
        }

        private void changeServer()
        {
            // Find a new server
        }

        private void setUp(string cURL, string sURL)
        {
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);
            myServer = (IServerServices)Activator.GetObject(
                typeof(IServerServices),
                sURL);
            myServer.NewUser(this.userName, cURL);
        }
    }
}
