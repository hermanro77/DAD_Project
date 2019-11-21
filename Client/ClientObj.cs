using MeetingCalendar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace Client
{
    public class ClientObj : MarshalByRefObject, IClientServices
    {
        private List<string> myCreatedMeetings = new List<string>();
        private List<IMeetingServices> meetingsClientKnows = new List<IMeetingServices>();
        private string userName;
        private string serverURL;
        private string myURL;
        IServerServices myServer;
        TcpChannel tcp;

        public ClientObj(string userName, string clientURL, string serverURL, string scriptFileName)
        {
            this.userName = userName;
            this.myURL = clientURL;
            string[] partlyURL = clientURL.Split(':');
            string[] endURL = partlyURL[partlyURL.Length - 1].Split('/');
            tcp = new TcpChannel(Int32.Parse(endURL[0]));

            Console.WriteLine("Client obj at: " + clientURL);
            Console.WriteLine("Creates connection to Server obj at: " + serverURL);
           
            ChannelServices.RegisterChannel(tcp, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ClientObj),
                userName,
                WellKnownObjectMode.Singleton);
            this.SetUpServer(clientURL, serverURL);
            // this.RunScript(scriptFileName);
        }

        public void RunScript(string scriptFileName)
        {
            
            string[] command;
            StreamReader script = new StreamReader(scriptFileName);
            while((command = script.ReadLine().Split(',')) != null)
            {
                switch (command[0])
                {
                    case "list":
                        this.ListMeetings();
                        break;
                    case "create":
                        this.createMeeting(command[1], Int32.Parse(command[2]),
                            this.ParseDateLoc(command, Int32.Parse(command[3])), this.ListInvitees(command));
                        break;
                    case "join":
                        this.JoinMeeting(command[1], this.ParseDateLoc(command, Int32.Parse(command[2])));
                        break;
                    case "close":
                        this.closeMeetingProposal(command[1]);
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

        private List<(string, DateTime)> ParseDateLoc(string[] command, int n)
        {
            List<(string, DateTime)> slots = new List<(string, DateTime)>();
            for (int i = 5; i < 5 + n; i++)
            {
                string[] dateLoc = command[i].Replace(@"(", "").Replace(@")", "").Split(';');
                slots.Add((dateLoc[0], DateTime.Parse(dateLoc[1])));
            }
            return slots;
        }

        private void wait(int delayTime)
        {
            // Not sure if works, temporary solution
            System.Threading.Thread.Sleep(delayTime);
        }
        private void createMeeting(string meetingTopic, int minAttendees,
            List<(string, DateTime)> slots, List<string> invitees)
        {
            try
            {
            IMeetingServices meetingProposal = new MeetingServices(this.userName, meetingTopic, minAttendees, slots, invitees);
                myServer.NewMeetingProposal(meetingProposal);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                this.changeServer();
            }
            // Create new meeting
            // USE TRY-CATCH
        }


        private List<string> getListOfClientURLs()
        {
            List<string> sample = myServer.getSampleClientsFromOtherServers();
            List<string> clientUnderSameServer = myServer.getClients();
            if (clientUnderSameServer.Contains(this.myURL)) {
                clientUnderSameServer.Remove(this.myURL); 
            }

            return sample.Concat(clientUnderSameServer).ToList();
        }

        private void JoinMeeting(string meetingTopic, List<(string, DateTime)> dateLoc)
        {
            try
            {
                myServer.JoinMeeting(meetingTopic, this.userName, true, dateLoc);
            } catch (Exception e) {
                Console.WriteLine(e);
                this.changeServer();
            }
        }

        private void closeMeetingProposal(string meetingTopic)
        {
            if (myCreatedMeetings.Contains(meetingTopic))
            {
                try
                {
                    myServer.closeMeetingProposal(meetingTopic, this.userName);
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                    changeServer();
                }
            }
        }

        private void ListMeetings()
        {
            try
            {
                List<IMeetingServices> availableMeetings = myServer.ListMeetings(userName,meetingsClientKnows, true);
                Console.WriteLine(availableMeetings);
            } catch (Exception e)
            {
                Console.WriteLine(e);
                this.changeServer();
            }
        }

        private void changeServer()
        {
            // Find a new server
        }

        private void SetUpServer(string cURL, string sURL)
        {
            this.myServer = (IServerServices)Activator.GetObject(
                typeof(IServerServices),
                sURL);
            
            myServer.NewClient(this.userName, cURL); 
        }

        public void PrintStatus()
        {
            Console.WriteLine("Client: " + userName + " My server is " + serverURL+".");
        }
        static void Main(string[] args)
        {
            new ClientObj(args[0], args[1], args[2], args[3]);
            Console.WriteLine("<enter> to exit...");
            Console.ReadLine();
        }
    }
}
