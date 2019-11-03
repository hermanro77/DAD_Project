using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace PuppetMaster
{
    class PuppetMaster
    {
        List<IServerServices> servers = new List<IServerServices>();
        List<IClientServices> clients = new List<IClientServices>();

        public List<IClientServices> Clients { get => clients; }

        public void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay)
        {

        }

        public void createClient(string username, string clientURL, string serverURL, string scriptFilePath)
        {

        }

        public void addRoom(string location, int capacity, string roomName)
        {

        }
        

    }
}
