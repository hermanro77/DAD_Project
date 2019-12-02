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

            void JoinMeeting(string meetingTopic, string userName, List<(string, DateTime)> dateLoc, int sequenceNumber);

            List<IMeetingServices> ListMeetings(string userName, List<IMeetingServices> meetingClientKnows, int sequenceNumber);
            Boolean closeMeetingProposal(string meetingTopic, string coordinatorUsername);
            void AddRoom(string location, int capacity, string roomName, int sequenceNumber);
            void AddNewServer(string URL);
            List<string> getSampleClientsFromOtherServers(int sequenceNumber);
            List<string> getOwnClients(int sequenceNumber);
            string getRandomClientURL(int sequenceNumber);
            void PrintStatus();
            List<IMeetingServices> getMeetings();
            string getServerURL();
            List<string> getOtherServerURLs();
            string getServerID();
            void serverKill();
            void freeze();
            void unfreeze();



        }
    }
}
