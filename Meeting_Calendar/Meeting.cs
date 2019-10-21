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
        // Dictionary with Client (participants) as key, and a List on the form [(location, date), (location, date).....]
        // The list is the participants possible dates and locations
        // Example: {"Maria": [("Lisbon","31/10/2019"), ("Lisbon","04/08/2019"), ("Porto","12/12/2020")], "Pedro": [("Porto", "05/09/2019")]}
        // Maybe it should rather be Dictionary<Client, Dictionary<Location, List>>
        private Dictionary<Client, List> participants = new Dictionary<Client, List>();
        // Dictionary for possible tuples of location and dates (string), set by the owner
        // Example: {"Lisbon": ["02/07/2019", "14/10/2020", "31/12/2020"], "Porto": ["05/05/2019", "10/11/2020"]}
        private Dictionary<Location, List> options;


        public MeetingServices(Client owner, String topic, int min_parts, Dictionary<Location, List> options)
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
