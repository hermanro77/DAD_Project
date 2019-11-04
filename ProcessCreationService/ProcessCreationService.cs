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
using Client;
using static CommonTypes.CommonType;
using System.Diagnostics;

namespace ProcessCreationService
{
    public class ProcessCreationService : IProcessCreationService
    { 
        public void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay, 
            string otherServerURL)
        {
            try
            {
                using (Process myProcess = new Process())
                {
                    
                    myProcess.StartInfo.FileName = "C:\\..\\Server\\bin\\Debug\\Server.exe";
                    myProcess.StartInfo.Arguments = serverID + " " + URL + " " + max_faults.ToString() + " " +
                        min_delay.ToString() + " " + max_delay.ToString();
                    myProcess.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            new ServerServices(otherServerURL, serverID, URL, max_faults, min_delay, max_delay);
        }
        
        public void createClient(string username, string clientURL, string serverURL, string scriptFilePath)
        {
            new ClientObj(username, clientURL, serverURL, scriptFilePath);   
        }

        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ProcessCreationService),
                "PCS",
                WellKnownObjectMode.Singleton);

            Console.WriteLine("<enter> to exit...");
            Console.ReadLine();

        }

    }
}
