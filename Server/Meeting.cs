using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace MeetingCalendar
{
    public class MeetingServices : MarshalByRefObject, IMeetingServices
    {
        private String coordinatorUsername;
        private String topic;
        private int minParticipants;
        private List<(String, DateTime)> locDateOptions;
        private List<String> participants = new List<String>();
        private Boolean closed;

        public MeetingServices(String username, String topic, int minParticipants, List<(string, DateTime)> slots, List<string> invitees)
        {
            this.coordinatorUsername = username;
            this.topic = topic;
            this.minParticipants = minParticipants;
            this.locDateOptions = slots;
            this.participants = invitees
            this.closed = false;
        }

        public string Topic { get => topic; }
        public int MinParticipants { get => minParticipants; }
        public List<(string, DateTime)> Slots { get => locDateOptions; }
        
        public Boolean Closed { set => closed = true;  }

        public void AddParticipant(string part) {
            if (!this.closed)
            {
                this.participants.Add(part);
            }
        }

        public void JoinMeeting(string userName)
        {
            return;
        }
    }
}
