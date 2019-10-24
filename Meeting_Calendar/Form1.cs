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

namespace MeetingCalendar
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

            myServer.NewUser();
        }

        internal void addMeetingToMeetingsList(String topic, int minParticipants, 
            List<(DateTime, String)> dateLocOptions) {
            
            foreach ((DateTime, String) option in dateLocOptions) {
                String[] itemArray = new String[3];
                itemArray[0] = topic;
                itemArray[1] = minParticipants.ToString();
                itemArray[2] = option.Item1.ToString() + " at " + option.Item2.ToString();
                ListViewItem item = new ListViewItem(itemArray);
                this.meetingsListView.Items.Add(item); //Create a component in form of type ListView
                                                        //with name meetingsListView
            }
        }
    }
}
