using System;
using System.Collections.Generic;
using static CommonTypes.CommonType;
using System.Net;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Threading;
using System.Net.Sockets;
using MeetingCalendar;
using System.IO;

namespace PuppetMaster
{
    class PuppetMaster
    {
        private delegate void InvokeDelegate(string updateString);
        public Form1 myForm;
        public PuppetMaster(Form1 form)
        {
            this.myForm = form;
        }
        public PuppetMaster()
        {

        }

        List<IServerServices> servers = new List<IServerServices>();
        List<string> serverURLs = new List<String>();

        List<IClientServices> clients = new List<IClientServices>();
        List<string> clientURLs = new List<String>();

        public List<IClientServices> Clients { get => clients; }

        public void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay, 
            string otherServerURL)
        {

            try
            {
                string[] URLsplit = URL.Split(':');
                IProcessCreationService PCS = (IProcessCreationService)Activator.GetObject(typeof(IProcessCreationService), URLsplit[0] + ":" + URLsplit[1] + ":10000/PCS");
                
                PCS.createServer(serverID, URL, max_faults, min_delay, max_delay, otherServerURL);

                AddNewServerToList(URL);
                serverURLs.Add(URL);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Could not create server");
            }
        }
        public void createClient(string username, string clientURL, string serverURL, string scriptFilePath)
        {
            try
            {
                string[] URLsplit = clientURL.Split(':');
                IProcessCreationService PCS = (IProcessCreationService)Activator.GetObject(typeof(IProcessCreationService), URLsplit[0] + ":" + URLsplit[1] + ":10000/PCS");

                PCS.createClient(username, clientURL, serverURL, scriptFilePath); 

                addClientToList(clientURL);
                clientURLs.Add(clientURL);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Could not create client");
            }

        }

        private string getPortFromURL(string url)
        {
            return url.Split(':')[2].Split('/')[0];
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private string chooseNeighboorServer()
        {
            if (serverURLs.Count == 0)
            {
                return "null";
            }
            else
            {
                string neighboorURL = serverURLs[serverURLs.Count - 1];
                return neighboorURL;
            }
        }

        public void addRoom(string location, int capacity, string roomName)
        {
             foreach (IServerServices server in servers)
            {
                server.AddRoom(location, capacity, roomName);
            }
        }

        public void Status()
        {
            foreach (IServerServices server in servers)
            {
                server.PrintStatus();
            }
            foreach (IClientServices client in clients)
            {
                client.PrintStatus();
            }
        }
        public void AddNewServerToList(string serverURL)
        {
            IServerServices server = (IServerServices)Activator.GetObject(typeof(IServerServices), serverURL);
            servers.Add(server);
        }

        public void addClientToList(string clientURL)
        {
            IClientServices client = (IClientServices)Activator.GetObject(typeof(IClientServices), clientURL);
            clients.Add(client);
        }

        public void crash(string serverId)
        {
            
            foreach (ServerServices server in servers)
            {
                if (server.getServerID() == serverId)
                {
                    server.serverKill();
                }
            }

        }

        public void freeze(string serverId)
        {
            foreach (ServerServices server in servers)
            {
                if (server.getServerID() == serverId)
                {
                    server.freeze();
                }
            }
        }

        public void unfreeze(string serverId)
        {
            foreach (ServerServices server in servers)
            {
                if (server.getServerID() == serverId)
                {
                    server.unfreeze();
                }
            }
        }

        public void wait(int millisec)
        {
            Thread.Sleep(millisec);
        }

        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(10001);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(PuppetMaster),
                "PCS",
                WellKnownObjectMode.Singleton);

            PuppetMaster PM = new PuppetMaster();

            Console.WriteLine("Write commands or paste path to script file (has to be .txt)" + Environment.NewLine + "Press <enter> to exit");
            string command = Console.ReadLine();
            if (command.Contains(".txt"))
            {
                string[] com;
                StreamReader script = new StreamReader(command);
                while ((com = script.ReadLine().Split(',')) != null)
                {
                    switch (com[0])
                    {
                        case "AddRoom":
                            PM.addRoom(com[1], Int32.Parse(com[2]), com[3]);
                            break;

                        case "Server":
                            PM.createServer(com[1], com[2], Int32.Parse(com[3]), Int32.Parse(com[4]), Int32.Parse(com[5]), com[6]);
                            break;

                        case "Client":
                            PM.createClient(com[1], com[2], com[3], com[4]);
                            break;

                        case "Status":
                            PM.Status();
                            break;

                        case "Wait":
                            PM.wait(Int32.Parse(com[1]));
                            break;

                        case "Freeze":
                            PM.freeze(com[1]);
                            break;

                        case "Unfreeze":
                            PM.unfreeze(com[1]);
                            break;

                        case "Crash":
                            PM.crash(com[1]);
                            break;

                        default:
                            break;
                    }
                }
            }
            else
            {
                do
                {
                    string[] com = command.Split(' ');

                    switch (com[0])
                    {
                        case "AddRoom":
                            PM.addRoom(com[1], Int32.Parse(com[2]), com[3]);
                            break;

                        case "Server":
                            PM.createServer(com[1], com[2], Int32.Parse(com[3]), Int32.Parse(com[4]), Int32.Parse(com[5]), com[6]);
                            break;

                        case "Client":
                            PM.createClient(com[1], com[2], com[3], com[4]);
                            break;

                        case "Status":
                            PM.Status();
                            break;

                        case "Wait":
                            PM.wait(Int32.Parse(com[1]));
                            break;

                        case "Freeze":
                            PM.freeze(com[1]);
                            break;

                        case "Unfreeze":
                            PM.unfreeze(com[1]);
                            break;

                        case "Crash":
                            PM.crash(com[1]);
                            break;

                        default:
                            break;
                    }
                    command = Console.ReadLine();
                } while (command != "");
            }
            
            
        }


    }

}
