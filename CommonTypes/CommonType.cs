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
            void NewUser();
            void NewMeetingProposal();
        }
    }
}
