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
            void myMeetingsFromServer(List<IMeetingServices> availableMeetings);
        }

        public interface IProcessCreationService
        {
            void createServer(string serverID, string URL, int max_faults, int min_delay, int max_delay, 
                string otherServerURLs);
            void createClient(string username, string clientURL, string serverURL, string scriptFilePath);

        }

        public interface IMeetingServices
        {
            void addUserToMeeting(string userName, List<(string, DateTime)> dateLoc);
            string getTopic();
            bool Eqauls(IMeetingServices meeting);
            void printStatus();
        }

        public interface IServerServices
        {
            void NewClient(string userName, string userURL, int sequenceNumber);
            void NewMeetingProposal(IMeetingServices proposal);

            void JoinMeeting(string meetingTopic, string userName, List<(string, DateTime)> dateLoc);

            void ListMeetings(string userName, string url, List<IMeetingServices> meetingClientKnows);
            Boolean closeMeetingProposal(string meetingTopic, string coordinatorUsername, int sequenceNumber);
            void AddRoom(string location, int capacity, string roomName, int sequenceNumber);
            void AddNewServer(string URL, int sequenceNumber = -1);
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
            void setSequencer(string sequencerURL);
            int handOutSeqNumber();
            int getHighestSeqNr();
            void electNewSequencer(IServerServices failedSequencer);
            void distributeAllMeetings();
            void failedServerDetected(string failedServerURL);
            void receiveMeetingProposal(IMeetingServices meeting, int systemSequenceNr);
            void addMeetingProposal(IMeetingServices proposal);
            int getSystemSequenceNumber();
            void incrementSqNum();
            void receiveJoinMeeting(string meetingTopic, string userName, List<(string, DateTime)> dateLoc, int seqNr);
            void receiveListMeeting(string userName, string url, List<IMeetingServices> meetingClientKnows, int seqNr);
            List<IMeetingServices> getMeetingsWhereInvited(string userName, List<IMeetingServices> meetingClientKnows);
        }
    }
}
