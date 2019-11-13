using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace MeetingCalendar
{
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
        public Boolean Closed { set => closed = true; }

        public bool IsInvited(string userName)
        {
            return invitees.Contains(userName);
        }
        public Dictionary<(string, DateTime), List<string>> Participants { get => participants; }

        public void AddParticipantToSlot((string, DateTime) slot, string part)
        {
            if (!this.closed)
            {
                this.participants[slot].Add(part);
            }
        }

        public void JoinMeeting(string userName, List<(string, DateTime)> dateLoc)
        {
            foreach ((string, DateTime) tuple in dateLoc)
            {
                this.AddParticipantToSlot(tuple, userName);
            }
        }

        public bool Equals(MeetingServices otherMeeting)
        {
            if (otherMeeting is null) return false;
            return this.topic == otherMeeting.Topic;
        }
        public override bool Equals(object obj) => Equals(obj as MeetingServices);
        public override int GetHashCode() => (Topic).GetHashCode();
    }

    //public class MeetingComparer : IEqualityComparer<MeetingServices>
    //{
    //    public bool Equals(MeetingServices x, MeetingServices y)
    //    {
    //        if (Object.ReferenceEquals(x, y)) return true;
    //        if ((Object.ReferenceEquals(x,null)) || (Object.ReferenceEquals(y,null))) return false;
    //        return (x.Topic == y.Topic);
    //    }

    //    public int GetHashCode(MeetingServices meeting)
    //    {
    //        if (Object.ReferenceEquals(meeting, null)) return 0;
    //        return meeting.Topic == null ? 0 : meeting.Topic.GetHashCode();
    //    }
    //}
}