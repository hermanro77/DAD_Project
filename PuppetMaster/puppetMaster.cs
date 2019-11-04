using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;
using System.Net;

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
        List<IServerServices> servers = new List<IServerServices>();
        List<string> serverURLs = new List<String>();

        List<IClientServices> clients = new List<IClientServices>();
       

        public List<IClientServices> Clients { get => clients; }

        public void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay)
        {
            try
                { AddNewServerToList(URL);
                serverURLs.Add(URL);

                string[] URLsplit = URL.Split(':');
                //sjekke om det er egen IP-adresse eller om
                if (URLsplit[1] == getIPAdress())
                {
                    //create server
                }
                else
                { 
                    IProcessCreationService PCS = (IProcessCreationService)Activator.GetObject(typeof(IProcessCreationService), URLsplit[1]+":10000");
                                PCS.createServer(serverID, URL, max_faults, min_delay, max_delay, chooseNeighboorServer());
                }

           
                //TODO: inform servers about the new server
                foreach (IServerServices server in servers)
                {
                    server.AddNewServer(URL);
                }

                //update GUI
                object[] serverstring = new object[1];
                serverstring[0] = "serverstring TODO";
                myForm.BeginInvoke(new InvokeDelegate(myForm.addServerListView), serverstring);

            }catch (Exception e)
            {
                Console.WriteLine("PM did not manage to create server");
            }
           
        }

        private string getIPAdress()
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST  
            // Get the IP  
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            Console.WriteLine("My IP Address is :" + myIP);
            Console.ReadKey();
            return "myIP";
        }

        private string chooseNeighboorServer()
        {
            if (serverURLs.Count == 0)
            {
                return null;
            }
            else
            {
                string neighboorURL = serverURLs[serverURLs.Count -1];
                return neighboorURL;
            }
        }

        public void createClient(string username, string clientURL, string serverURL, string scriptFilePath)
        {
            try
            {
                string[] URLsplit = serverURL.Split(':');
                //sjekke om det er egen IP-adresse eller om
                if (URLsplit[1] == getIPAdress())
                {
                    //create client
                }
                else
                {
                    IProcessCreationService PCS = (IProcessCreationService)Activator.GetObject(typeof(IProcessCreationService), processCreationServiceURLs[indexOfPCS]);
                PCS.createClient(username, clientURL, serverURL, scriptFilePath);

                }
                //Update GUI
                object[] clientstring = new object[1];
                clientstring[0] = "clientstring ";
                myForm.BeginInvoke(new InvokeDelegate(myForm.addClientListView), clientstring);

            }catch (Exception e)
            {
                Console.WriteLine();
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

    }
     
}
