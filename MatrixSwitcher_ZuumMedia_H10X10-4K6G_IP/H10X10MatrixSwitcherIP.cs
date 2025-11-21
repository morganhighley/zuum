using System;
using System.Net;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.DeviceTypes.CableBox;
using Crestron.SimplSharp;

namespace Crestron.RAD.Drivers.MatrixSwitchers.ZuumMedia
{
    public class H10X10MatrixSwitcherIP : ACableBox, ITcp, ISimpl
    {
        #region ITcp Members

        public void Initialize(IPAddress ipAddress, int port)
        {
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

        /// <summary>
        /// SIMPL+ initialization
        /// </summary>
        public ushort EnableSimpl()
        {
            return 1;
        }

        #endregion
    }
}

