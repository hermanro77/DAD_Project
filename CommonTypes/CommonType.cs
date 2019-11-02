﻿using System;
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
            void JoinMeeting();

        }

        public interface IServerServices
        {
            void NewUser(string userName, int port_number);
            void NewMeetingProposal(IMeetingServices proposal);

            Boolean closeMeetingProposal(string meetingTopic, string coordinatorUsername);
        }
    }
}
