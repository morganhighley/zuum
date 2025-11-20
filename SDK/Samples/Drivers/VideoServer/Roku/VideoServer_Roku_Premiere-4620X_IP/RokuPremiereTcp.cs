// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="RokuPremiereTcp.cs">
//   
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------

namespace Crestron.RAD.Drivers.VideoServers
{
    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.SimplSharp;
    using Crestron.RAD.DeviceTypes.VideoServer;
    using Crestron.SimplSharp.Reflection;
    using System.Linq;

    public class RokuPremiereTcp : ABasicVideoServer, IHttp, ITcp
    {
        private int _serverPollingIntervalInMs = 30000;
        private CTimer _serverPoll;
        private ulong _pollCount = 0;
        internal bool PollSent = false;

        public void Initialize(IPAddress ipAddress, int port)
        {
            var httpTransport = new RokuPremiereTcpHttpTransport(this)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };

            httpTransport.Initialize(ipAddress, port);
            ConnectionTransport = httpTransport;

            VideoServerProtocol = new RokuPremiereProtocol(ConnectionTransport, Id, this)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            VideoServerProtocol.StateChange += StateChange;
            VideoServerProtocol.RxOut += SendRxOut;
            VideoServerProtocol.Initialize(VideoServerData);

            DetermineHttpTransportVersion();
        }

        private void DetermineHttpTransportVersion()
        {
            //CrestronConsole.PrintLine("In DetermineTransportVersion");
            CType type = typeof(HttpTransport);
            var methods = type.GetMethods().ToList();
            var transportIsAsync = methods.Find(x => x.Name == "HostResponseCallback");
            if (transportIsAsync != null)
            {
                //CrestronConsole.PrintLine("In DetermineTransportVersion Polls will be sent");
                // Only poll if Common is using the Async version of HttpTransport
                // The Async version introduces a new public method called HostResponse
                _serverPoll = new CTimer(PollConnection, null, 0, _serverPollingIntervalInMs);
            }
            else
            {
                //CrestronConsole.PrintLine("In DetermineTransportVersion Polls will be NOT be sent");
            }
        }

        private void PollConnection(object obj)
        {
            //CrestronConsole.PrintLine(" Sending CustomCommand Poll");
            if (_pollCount > 0)
            {
                SendCustomCommand("query/active-app");
            }
            _pollCount++;

            if (_pollCount >= 2)
            {
                PollSent = true;
            }
        }

        public override void Dispose()
        {
            if (_serverPoll != null &&
                !_serverPoll.Disposed)
            {
                _serverPoll.Stop();
                _serverPoll.Dispose();
            }

            base.Dispose();
        }
    }
}
