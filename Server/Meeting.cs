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
        private List<(DateTime, String)> dateLocOptions;
        private List<String> participants = new List<String>();
        private Boolean closed;

        public MeetingServices(String username, String topic, int minParticipants, DateTime date,
            String location)
        {
            this.coordinatorUsername = username;
            this.topic = topic;
            this.minParticipants = minParticipants;
            (DateTime, String) dateLocOption = (date, location);
            this.dateLocOptions.Add(dateLocOption);
            this.closed = false;
        }

        public string Topic { get => topic; }
        public int MinParticipants { get => minParticipants; }
        public List<(DateTime, string)> DateLocOptions { get => dateLocOptions; }
        
        public Boolean Closed { set => closed = true;  }

        public void AddParticipant(string part) {
            if (!this.closed)
            {
                this.participants.Add(part);
            }
        }

        public void JoinMeeting()
        {
            return;
        }
    }
}
