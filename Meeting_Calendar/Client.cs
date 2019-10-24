using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace MeetingCalendar
{
    
    public class ClientService : MarshalByRefObject, IClientServices
    {
        private Form1 myForm;
        private delegate void InvokeDelegate(String topic, int minParticipants,
            List<(DateTime, String)> dateLocOptions);
        public ClientService(Form1 form1)
        {
            this.myForm = form1;
        }

        // Notification about a new meeting proposal, might wanna join
        public void NewProposal()
        {
            throw new NotImplementedException();
        }

        public void showAvailableMeetings(Meeting meeting) {
            object[] meetingParams = new object[3];
            meetingParams[0] = meeting.Topic;
            meetingParams[1] = meeting.MinParticipants;
            meetingParams[2] = meeting.DateLocOptions;
            myForm.BeginInvoke(new InvokeDelegate(myForm.addMeetingToMeetingsList), meetingParams);
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
