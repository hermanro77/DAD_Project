﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

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
        List<string> processCreationServiceURLs = new List<string>();

        int createServerCount;
        int createClientCount;

        public List<IClientServices> Clients { get => clients; }

        public void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay)
        {
            AddNewServerToList(URL);
            serverURLs.Add(URL);

           
            int indexOfPCSURL = createServerCount % processCreationServiceURLs.Count;
            IProcessCreationService PCS = (IProcessCreationService).Activator.GetObject(typeof(IProcessCreationService), processCreationServiceURLs[indexOfPCSURL]);
            PCS.createServer(serverID, URL, max_faults, min_delay, max_delay, serverURLs);
           
            //todo: add parameters
            //TODO: inform servers about the new server
            foreach (IServerServices server in servers)
            {
                server.AddNewServer(URL);
            }

            //update GUI
            object[] serverstring = new object[1];
            serverstring[0] = "serverstring TODO";
            myForm.BeginInvoke(new InvokeDelegate(myForm.addServerListView), serverstring);
        }

        public void createClient(string username, string clientURL, string serverURL, string scriptFilePath)
        {
            int indexOfPCS = createClientCount % processCreationServices.Count;
            IProcessCreationService PCS = (IProcessCreationService).Activator.GetObject(typeof(IProcessCreationService), processCreationServiceURLs[indexOfPCSURL]);
            PCS.createClient(username, clientURL, serverURL, scriptFilePath);


            //Update GUI
            object[] clientstring = new object[1];
            clientstring[0] = "clientstring ";
            myForm.BeginInvoke(new InvokeDelegate(myForm.addClientListView), clientstring);
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
