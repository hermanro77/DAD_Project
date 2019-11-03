using System;
using System.Collections.Generic;

namespace CommonTypes
{
    public class CommonType : MarshalByRefObject
    {
        public interface IClientServices
        {
            void NewProposal(string uid);
        }

        public interface IProcessCreationService
        {
            void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay, 
                List<string> otherServerURLs);
            void createClient(string username, string clientURL, string serverURL, string scriptFilePath);

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
        }
    }
}
