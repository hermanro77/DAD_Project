using System;
using System.Collections.Generic;

namespace CommonTypes
{
    public class CommonType
    {
        public interface IClientServices
        {
            
            void PrintStatus();
        }
        public interface IProcessCreationService
        {
            void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay,
                List<string> otherServerURLs);
            void createClient(string username, string clientURL, string serverURL, string scriptFilePath);

        }
        public interface IMeetingServices
        {
            void JoinMeeting(string userName);

        }

        public interface IServerServices
        {
            void NewClient(string userName, string userURL);
            void NewMeetingProposal(IMeetingServices proposal);

            void JoinMeeting(string meetingTopic, string userName, 
                bool requesterIsClient, List<(string, DateTime)> dateLoc);

            List<IMeetingServices> ListMeetings(string userName, bool requesterIsClient);
            Boolean closeMeetingProposal(string meetingTopic, string coordinatorUsername);
            void AddRoom(string location, int capacity, string roomName);
            void AddNewServer(string URL);
            void PrintStatus();
        }
    }
}
