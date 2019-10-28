using System;
using System.Collections.Generic;
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
            // open script cile
            // insert while loop here
            // line = readline.next()
            // switch(line):
            //  case1("createMeeting"):
            //      this.createMeeting()
            //      break
            //  case2("wait"):
            //      this.wait()
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
