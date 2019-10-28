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

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {

        }

        private void seeAvailableMeetings_Click(object sender, EventArgs e)
        {

        }

        private void joinSelectedMeetings_Click(object sender, EventArgs e)
        {

        }

        private void addDateAndLocation_Click(object sender, EventArgs e)
        {

        }

        private void sendMeetingProposal_Click(object sender, EventArgs e)
        {

        }

        internal void addMeetingToMeetingsList(String topic, int minParticipants, 
            List<(DateTime, String)> dateLocOptions) {
            
            foreach ((DateTime, String) option in dateLocOptions) {
                String[] itemArray = new String[3];
                itemArray[0] = topic;
                itemArray[1] = minParticipants.ToString();
                itemArray[2] = option.Item1.ToString() + " at " + option.Item2.ToString();
                ListViewItem item = new ListViewItem(itemArray);
                this.listView1.Items.Add(item); //Create a component in form of type ListView
                                                        //with name meetingsListView
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
