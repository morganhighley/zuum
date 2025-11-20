// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Crestron Electronics" file="SonyXBRSeriesTcp.cs">
//   
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------


namespace Crestron.RAD.Drivers.Displays
{
    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.SimplSharp;
    using Crestron.RAD.DeviceTypes.Display;

    public class SonyXBRSeriesTcp : ABasicVideoDisplay, ITcp
    {
        private SonyXBRSeriesTcpProtocol _protocol;
        public SonyXBRSeriesTcp() { }
        public void Initialize(IPAddress ipAddress, int port)
        {
            var httpTransport = new SonyXBRSeriesTcpHttpTransport
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };

            httpTransport.Initialize(ipAddress, port);
            
            ConnectionTransport = httpTransport;
            
            DisplayProtocol = new SonyXBRSeriesTcpProtocol(ConnectionTransport, Id, ipAddress)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            DisplayProtocol.StateChange += StateChange;
            DisplayProtocol.RxOut += SendRxOut;
            DisplayProtocol.Initialize(DisplayData);

            _protocol = DisplayProtocol as SonyXBRSeriesTcpProtocol;
            httpTransport.SetProtocol(ref _protocol);
        }
    }
}

