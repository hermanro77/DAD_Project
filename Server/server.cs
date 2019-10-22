using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using static CommonTypes.CommonType;

namespace Server
{
    public class ServerServices : MarshalByRefObject, IServerServices
    {
        public void NewMeetingProposal()
        {
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(IMeetingServices),
                "SomeNewMeeting",
                WellKnownObjectMode.Singleton);
            // throw new NotImplementedException();
        }

        public void NewUser()
        {
            throw new NotImplementedException();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(8086);

            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(IServerServices),
                "MyRemoteServer",
                WellKnownObjectMode.Singleton);

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
}
