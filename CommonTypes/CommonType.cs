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
            void JoinAttendence();

            void CloseProposal(IClientServices client);
        }

        public interface IServerServices
        {
            void NewUser(string userName, int port_number);
            void NewMeetingProposal(string uid, List<string> parts);
        }
    }
}
