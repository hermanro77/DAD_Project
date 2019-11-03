using MeetingCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace ProcessCreationService
{
    class ProcessCreationService : IProcessCreationService
    {
        public void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay, 
            List<string> otherServerURLs)
        {
            ServerServices server = new ServerServices(serverID, URL, otherServerURLs);
        }

        public void createClient(string username, string clientURL, string serverURL, string scriptFilePath)
        {

        }

        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(IProcessCreationService),
                "ProcessCreationRemoteService",
                WellKnownObjectMode.Singleton);

            Console.WriteLine("<enter> to exit...");
            Console.ReadLine();

        }

    }
}
