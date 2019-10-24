using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CommonTypes.CommonType;

namespace Meeting_Calendar
{
    public partial class Form1 : Form
    {

        TcpChannel tcp;
        IServerServices myServer;

        public Form1()
        {
            InitializeComponent();
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            // Dummy method
            Console.WriteLine("Hello");
        }

        private void registerNewClient()
        {
            //int port = Int32.Parse(Port_number.Text);
            // tcp = new TcpChannel(port);
            ChannelServices.RegisterChannel(tcp, false);
            ClientService cs = new ClientService(this);
            RemotingServices.Marshal(cs, "MyRemoteClient", typeof(ClientService));
            this.myServer = (IServerServices)Activator.GetObject(
            typeof(IServerServices),
            "tcp://localhost:8086/MyRemoteServer");

            myServer.NewUser(this.username, this.portnumber);
        }

        private void registerNewProposal()
        {
            myServer.NewMeetingProposal(uniqueMeetingID);
            MeetingServices newMeeting = new MeetingServices(starter, topic, min_participants, List<Dates>);
            RemotingServices.Marshal(newMeeting, "RemoteMeeting" + uniqueMeetingID, typeof(MeetingServices));
        }
    }
}
