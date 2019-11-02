using System;
using System.Collections.Generic;

namespace CommonTypes
{
    public class CommonType
    {
        public interface IClientServices
        {
            void NewProposal(string uid);
        }

        public interface IMeetingServices
        {
            void JoinMeeting(string userName);

        }

        public interface IServerServices
        {
            void NewUser(string userName, int port_number);
            void NewMeetingProposal(IMeetingServices proposal);

            void CloseMeetingProposal(string meetingTopic, string coordinatorUsername);

            void JoinMeeting(string meetingTopic, string userName, 
                bool requesterIsClient, List<(string, DateTime)> dateLoc);
        }
    }
}
