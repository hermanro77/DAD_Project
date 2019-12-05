using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace MeetingCalendar
{
    [Serializable]
    public class MeetingServices : MarshalByRefObject, IMeetingServices, IEquatable<MeetingServices>
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
        public bool Closed { 
            get { return closed; } 
            set { closed = value; } 
        }

        public Dictionary<(string, DateTime), List<string>> Participants { get => participants; }
        public string getTopic()
        {
            return this.Topic;
        }
        public bool IsInvited(string userName)
        {
            if (invitees == null)
            {
                return true;
            }
            return invitees.Contains(userName);
        }
        

        public void AddParticipantToSlot((string, DateTime) slot, string part)
        {
            if (!this.closed)
            {
                if (participants.Keys.Contains(slot)) {
                    if (!participants[slot].Contains(part))
                    {
                        this.participants[slot].Add(part);
                    }
                }else
                {
                    this.participants[slot] = new List<string>() { part };
                }
            }
        }

        public void addUserToMeeting(string userName, List<(string, DateTime)> dateLoc)
        {
            foreach ((string, DateTime) tuple in dateLoc)
            {
                this.AddParticipantToSlot(tuple, userName);
            }
        }

        public void printStatus()
        {
            Console.WriteLine("Dates and locations set by the coordinator: ");
            foreach((string, DateTime) dateLoc in Slots)
            {
                Console.WriteLine("Place: " + dateLoc.Item1 + ",  date: " + dateLoc.Item2.ToString("d"));
            }
            Console.WriteLine("Meeting topic: " + topic);
            Console.WriteLine("Meeting closed: " + closed);
            //Console.WriteLine("Possible places and dates: ");
            foreach (KeyValuePair<(string, DateTime), List<string>> probMeeting in participants)
            {
                Console.WriteLine("Location: " + probMeeting.Key.Item1 + ", Date: " + probMeeting.Key.Item2 +
                    ", number of people who can attend at this date and place: " + probMeeting.Value.Count);
            }
            Console.WriteLine("Coordinator of meeting: " + coordinatorUsername);
        }

        public override bool Equals(object obj) => Equals(obj as MeetingServices);
        public override int GetHashCode() => (Topic).GetHashCode();

        public bool Eqauls(IMeetingServices meeting)
        {
            if (meeting is null) return false;
            return this.topic == ((MeetingServices)meeting).Topic;
        }
        public bool Equals(MeetingServices other)
        {
            if (other is null) return false;
            return this.topic == other.Topic;
        }
    }
}