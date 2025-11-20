// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="TivoTcp.cs">
//   
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------

using Crestron.RAD.DeviceTypes.CableBox;

namespace Crestron.RAD.Drivers.CableBoxes
{
    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.SimplSharp;

    public class TivoTcp : ABasicCableBox, ITcp
    {
        public void Initialize(IPAddress ipAddress, int port)
        {
            var tcpTransport = new TcpTransport
            {
                EnableAutoReconnect = EnableAutoReconnect,
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
                
            };
            tcpTransport.Initialize(ipAddress, port);
            ConnectionTransport = tcpTransport;

            CableBoxProtocol = new TivoProtocol(ConnectionTransport, Id)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            CableBoxProtocol.StateChange += StateChange;
            CableBoxProtocol.RxOut += SendRxOut;

            CableBoxProtocol.Initialize(CableBoxData);
        }
    }
}
