using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace Meeting_Calendar
{
    public class MeetingServices : MarshalByRefObject, IMeetingServices
    {
        private Client coordinator;
        private String topic;
        private int min_participants;


        public MeetingServices(Client owner, String topic, int min_parts, Dictionary options)
        {
            this.coordinator = owner;
            this.topic = topic;
            this.min_participants = min_parts;
            this.options = options;
        }

        public void CloseProposal(IClientServices client)
        {
            // Do some scheduling
            // Call Location to figure out which room
            // If no room available, cancel the meeting
        }

        public void JoinAttendence(Client client, List dates)
        {
            if (!participants.ContaintsKey(client))
            {
                participants.add(client, dates);
            }
        }

        public void JoinAttendence()
        {
            throw new NotImplementedException();
        }
    }
    public class Meeting
    {
        public Meeting()
        {
        }

        public void closeProposal()
        {
            // Do some scheduling
            // Call Location to figure out which room
            // If no room available, cancel the meeting
        }
    }
}
