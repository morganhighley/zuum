 
using Crestron.RAD.DeviceTypes.Display;


 
namespace Crestron.RAD.Drivers.Displays
{
    using System;
    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.ProTransports;
    
    public class SonyXBRSeriesSerial : ABasicVideoDisplay, ISerialComport, ISimpl
    {
        private SimplTransport _transport;

        public SonyXBRSeriesSerial()
        {}
        public void Initialize(IComPort comPort)
        {
            ConnectionTransport = new CommonSerialComport(comPort)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };

            DisplayProtocol = new SonyXBRSeriesProtocol(ConnectionTransport, Id)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            DisplayProtocol.StateChange += StateChange;
            DisplayProtocol.RxOut += SendRxOut;
            DisplayProtocol.Initialize(DisplayData);
        }

        public SimplTransport Initialize(Action<string, object[]> send)
        {
            _transport = new SimplTransport { Send = send };
            ConnectionTransport = _transport;

            DisplayProtocol = new SonyXBRSeriesProtocol(ConnectionTransport, Id);
            DisplayProtocol.StateChange += StateChange;
            DisplayProtocol.RxOut += SendRxOut;
            DisplayProtocol.Initialize(DisplayData);

            return _transport;
        }
    }
}

       