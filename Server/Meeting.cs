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
        private List<String> invitees;
        private List<(string, DateTime)> locDateOptions;
        private Dictionary<(string, DateTime), List<string>> participants = new Dictionary<(string, DateTime), List<string>>();
        private Boolean closed;

        public MeetingServices(String username, String topic, int minParticipants, List<(string, DateTime)> slots, List<string> invitees)
        {
            this.coordinatorUsername = username;
            this.topic = topic;
            this.minParticipants = minParticipants;
            this.locDateOptions = slots;
            this.invitees = invitees;
            this.closed = false;
        }

        public string Topic { get => topic; }
        public int MinParticipants { get => minParticipants; }
        public List<(string, DateTime)> LocDateOptions { get => locDateOptions; }
        public List<(string, DateTime)> Slots { get => locDateOptions; }
        public Boolean Closed { set => closed = true;  }
        
        public bool IsInvited(string userName)
        {
            return participants.Contains(userName);
        }
        public Dictionary<(string, DateTime), List<string>> Participants { get => participants; }

        public void AddParticipantToSlot((string, DateTime) slot, string part) {
            if (!this.closed)
            {
                this.participants[slot].Add(part);
            }
        }

        public void JoinMeeting(string userName)
        {
            return;
        }
    }
}
