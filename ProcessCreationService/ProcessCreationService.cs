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
using System.Diagnostics;

namespace ProcessCreationService
{
    public class ProcessCreationService : IProcessCreationService
    { 
        public ProcessCreationService()
        {

        }
        public void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay, 
            string otherServerURL)
        {
            try
            {
                using (Process ServerProcess = new Process())
                {
                    ServerProcess.StartInfo.FileName = "..\\..\\..\\Server\\bin\\Debug\\Server.exe";
                    
                    ServerProcess.StartInfo.Arguments = otherServerURL + " " + serverID + " " + URL + " " +
                        max_faults.ToString() + " " +
                        min_delay.ToString() + " " + 
                        max_delay.ToString();
                    ServerProcess.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not initiate new Server Process");
                Console.WriteLine(e.Message);
            }
        }
        
        public void createClient(string username, string clientURL, string serverURL, string scriptFilePath)
        {
            try
            {
                using (Process ClientProcess = new Process())
                {

                    ClientProcess.StartInfo.FileName = "..\\..\\..\\Client\\bin\\Debug\\Client.exe";
                    ClientProcess.StartInfo.Arguments = username + " " + clientURL + " " + serverURL + " " + 
                        scriptFilePath;
                    ClientProcess.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not initiate new Client Process");
                Console.WriteLine(e.Message);
            }
        }

        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ProcessCreationService),
                "PCS",
                WellKnownObjectMode.Singleton);

            //Creates 3 servers and 1 client and set them up to know each other
            //ProcessCreationService pcs = new ProcessCreationService();
            //pcs.createServer("server1", "tcp://localhost:50000/server1", 3, 0, 0, "null");
            //pcs.createServer("server2", "tcp://localhost:50012/server2", 3, 0, 0, "tcp://localhost:50000/server1");
            //pcs.createServer("server3", "tcp://localhost:50013/server3", 3, 0, 0, "tcp://localhost:50012/server2");

            //Thread.Sleep(10000); //Waits for 10 seconds, needs to wait for server setup
            //pcs.createClient("client1", "tcp://localhost:50001/client1", "tcp://localhost:50000/server1", @"C:\Users\larsm\Distribuerte\DAD_Project\Client\scriptFile.txt");
            //pcs.createClient("client2", "tcp://localhost:50002/client2", "tcp://localhost:50000/server1", "scriptFilePath");

            //Process[] processes = Process.GetProcessesByName("Server", "localhost");
            //foreach(Process process in processes)
            //{
             //   Console.WriteLine(process.ProcessName);
            //}
            Console.WriteLine("<enter> to exit...");
            Console.ReadLine();

        }

    }
}
