using System;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.DeviceTypes.CableBox;
using Crestron.SimplSharp;
using IPAddress = Crestron.SimplSharp.IPAddress;

namespace Crestron.RAD.Drivers.MatrixSwitchers.ZuumMedia
{
    public class H10X10MatrixSwitcherIP : ACableBox, ITcp, ISimpl
    {
        #region ITcp Members

        public int Port { get; private set; }

        public void Initialize(IPAddress ipAddress, int port)
        {
            Port = port;

            var tcpTransport = new TcpTransport
            {
                EnableAutoReconnect = true,
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug,
                Host = ipAddress.ToString(),
                Port = (ushort)port
            };

            ConnectionTransport = tcpTransport;

            CableBoxProtocol = new H10X10MatrixSwitcherProtocol(ConnectionTransport, Id)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            CableBoxProtocol.StateChange += StateChange;
            CableBoxProtocol.RxOut += SendRxOut;
            CableBoxProtocol.Initialize(CableBoxData);
        }

        #endregion

        #region ISimpl Members

        public ISimplTransport Initialize(Action<string, object[]> send)
        {
            var simplTransport = new SimplTransport(send)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };

            ConnectionTransport = simplTransport;

            CableBoxProtocol = new H10X10MatrixSwitcherProtocol(ConnectionTransport, Id)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            CableBoxProtocol.StateChange += StateChange;
            CableBoxProtocol.RxOut += SendRxOut;
            CableBoxProtocol.Initialize(CableBoxData);

            return simplTransport;
        }

        #endregion
    }
}

