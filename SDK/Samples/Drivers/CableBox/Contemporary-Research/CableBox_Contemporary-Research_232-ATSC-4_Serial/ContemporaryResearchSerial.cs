// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="ContemporaryResearchSerial.cs">
//   
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------

using Crestron.RAD.DeviceTypes.CableBox;

namespace Crestron.RAD.Drivers.CableBoxes
{
    using System;

    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.ProTransports;

    public class ContemporaryResearchSerial : ABasicCableBox, ISerialComport, ISimpl
    {
        public void Initialize(IComPort comPort)
        {
            ConnectionTransport = new CommonSerialComport(comPort)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };

            CableBoxProtocol = new ContemporaryResearchProtocol(ConnectionTransport, Id)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            CableBoxProtocol.StateChange += StateChange;
            CableBoxProtocol.RxOut += SendRxOut;
            CableBoxProtocol.Initialize(CableBoxData);
        }      

        public SimplTransport Initialize(Action<string, object[]> send)
        {
            _transport = new SimplTransport { Send = send };
            ConnectionTransport = _transport;

            CableBoxProtocol = new ContemporaryResearchProtocol(ConnectionTransport, Id);
            CableBoxProtocol.StateChange += StateChange;
            CableBoxProtocol.RxOut += SendRxOut;

            CableBoxProtocol.Initialize(CableBoxData);
            return _transport;
        }

        private SimplTransport _transport;
        public override bool SupportsDisconnect
        {
            get { return false; }
        }

        public override bool SupportsReconnect
        {
            get { return false; }
        }
    }
}
