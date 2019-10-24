using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace Meeting_Calendar
{
    
    public class ClientService : MarshalByRefObject, IClientServices
    {
        private Form1 myForm;
        public ClientService(Form1 form1)
        {
            this.myForm = form1;
        }

        // Notification about a new meeting proposal, might wanna join
        public void NewProposal()
        {
            throw new NotImplementedException();
        }
    }
    public class Client
    {
        private IServerServices myServer;
        public Client()
        {
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);
            myServer = (IServerServices)Activator.GetObject(
                typeof(IServerServices),
                "tcp://localhost:8086/MyRemoteServer");
        }
    }
}
