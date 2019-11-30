using System;
using System.Collections.Generic;

namespace CommonTypes
{
    public class CommonType : MarshalByRefObject
    {
        public interface IClientServices
        {
            
            void PrintStatus();
            void receiveNewMeeting(IMeetingServices meetingProposal, bool forwardMeeting);
        }

        public interface IProcessCreationService
        {
            void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay, 
                string otherServerURLs);
            void createClient(string username, string clientURL, string serverURL, string scriptFilePath);

        }

        public interface IMeetingServices
        {
            void JoinMeeting(string userName, List<(string, DateTime)> dateLoc);
            string getTopic();
            bool Eqauls(IMeetingServices meeting);
            void printStatus();
        }

        public interface IServerServices
        {
            void NewClient(string userName, string userURL);
            void NewMeetingProposal(IMeetingServices proposal);

            void JoinMeeting(string meetingTopic, string userName, 
                bool requesterIsClient, List<(string, DateTime)> dateLoc);

            List<IMeetingServices> ListMeetings(string userName, List<IMeetingServices> meetingClientKnows, bool requesterIsClient);
            Boolean closeMeetingProposal(string meetingTopic, string coordinatorUsername);
            void AddRoom(string location, int capacity, string roomName);
            void AddNewServer(string URL);
            List<string> getSampleClientsFromOtherServers();
            List<string> getOwnClients();
            string getRandomClientURL();
            void PrintStatus();
            List<IMeetingServices> getMeetings();
            string getServerURL();
            List<string> getOtherServerURLs();
            string getServerID();
            void serverKill();
            void freeze();
            void unfreeze();
            void setSequencer(string sequencerURL);
            int handOutSeqNumber();

        }
    }
}
