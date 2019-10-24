using System;

namespace CommonTypes
{
    public class CommonType
    {
        public interface IClientServices
        {
            void NewProposal();
        }

        public interface IMeetingServices
        {
            void JoinAttendence();

            void CloseProposal(IClientServices client);
        }

        public interface IServerServices
        {
            void NewUser(string userName, int port_number);
            void NewMeetingProposal(string uid);
        }
    }
}
