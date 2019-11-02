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
        private Client coordinator;
        


        public MeetingServices(Client owner, String topic, int minParts, Dictionary options)
        {
            this.coordinator = owner;
            this.topic = topic;
            this.minParticipants = min_parts;
            this.options = options;
        }

        public void CloseProposal(IClientServices client)
        {
            // Do some scheduling
            // Call Location to figure out which room
            // If no room available, cancel the meeting
        }

        //public void JoinAttendence(Client client, List dates)
        //{
        //    if (!participants.ContaintsKey(client))
        //    {
        //        participants.add(client, dates);
        //    }
        //}

        public void JoinAttendence()
        {
            throw new NotImplementedException();
        }
    }
    public class Meeting {
        
        private String topic;
        private int minParticipants;
        private List<(DateTime, String)> dateLocOptions;
        public Meeting(String topic, int minParticipants, DateTime date, String location) {
            this.topic = topic;
            this.minParticipants = minParticipants;
            (DateTime, String) dateLocOption = (date, location);
            this.dateLocOptions.Add(dateLocOption);
        }

        public string Topic { get => topic; }
        public int MinParticipants { get => minParticipants; }
        public List<(DateTime, string)> DateLocOptions { get => dateLocOptions; }

        public void closeProposal()
        {
            // Do some scheduling
            // Call Location to figure out which room
            // If no room available, cancel the meeting
        }
    }
}
