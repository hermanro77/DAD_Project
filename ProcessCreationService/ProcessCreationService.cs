using MeetingCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CommonTypes.CommonType;
using Client;

namespace ProcessCreationService
{
    class ProcessCreationService : IProcessCreationService
    {
        private delegate void InvokeDelegate(string scriptFilePath);
        public void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay, 
            List<string> otherServerURLs)
        {
            ServerServices server = new ServerServices(otherServerURLs, serverID, URL);
            Thread thread = new Thread(new ThreadStart(server.initServer)); //Create initServer method
            thread.Start();
        }

        public void createClient(string username, string clientURL, string serverURL, string scriptFilePath)
        {
            object path = (object)scriptFilePath;
            ClientObj client = new ClientObj(username, clientURL, serverURL, scriptFilePath);
            Thread thread = new Thread(new ParameterizedThreadStart(client.RunScript));
            thread.Start(scriptFilePath);
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
