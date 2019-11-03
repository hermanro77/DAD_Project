using System;
using System.Collections.Generic;

namespace CommonTypes
{
    public class CommonType
    {
        public interface IClientServices
        {
            void NewProposal(string uid);
            void PrintStatus();
        }

        public interface IMeetingServices
        {
            void JoinMeeting();

        }

        public interface IServerServices
        {
            void NewUser(string userName, int port_number);
            void NewMeetingProposal(IMeetingServices proposal);

            void closeMeetingProposal(string meetingTopic, string coordinatorUsername);
            void AddRoom(string location, int capacity, string roomName);
            void AddNewServer(string URL);
            void PrintStatus();
        }
    }
}
