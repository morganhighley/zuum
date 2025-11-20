// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EpsonPowerLiteComport.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// For Basic SIMPL# Classes

// For Basic SIMPL#Pro classes

namespace Crestron.RAD.Drivers.Displays
{
    using System;

    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.ProTransports;
    using Crestron.RAD.DeviceTypes.Display;

    public class EpsonPowerLiteComport : ABasicVideoDisplay, ISerialComport, ISimpl
    {
        public EpsonPowerLiteComport()
        {
        }

        public void Initialize(IComPort comPort)
        {
            ConnectionTransport = new CommonSerialComport(comPort)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };

            DisplayProtocol = new EpsonPowerLiteProtocol(ConnectionTransport, Id)
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

            DisplayProtocol = new EpsonPowerLiteProtocol(ConnectionTransport, Id);
            DisplayProtocol.StateChange += StateChange;

            DisplayProtocol.RxOut += SendRxOut;
            DisplayProtocol.Initialize(DisplayData);

            return _transport;
        }

        private SimplTransport _transport;
    }
}

