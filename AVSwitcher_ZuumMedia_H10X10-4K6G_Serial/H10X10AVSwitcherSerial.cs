// Zuum Media H10X10-4K6G HDMI Matrix Switcher - Serial Driver
// 4K60Hz 10x10 HDMI Matrix Switcher with Audio Extraction
// Crestron Home AV Switcher Driver for 4-Series Processors

namespace AVSwitcher_ZuumMedia_H10X10_4K6G_Serial
{
    using System;
    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.AudioVideoSwitcher;
    using Crestron.RAD.ProTransports;

    /// <summary>
    /// Zuum Media H10X10-4K6G HDMI Matrix Switcher RS-232 Serial Driver.
    /// Inherits from AAudioVideoSwitcher for proper Crestron Home integration as an AV Switcher device type.
    /// </summary>
    public class H10X10AVSwitcherSerial : AAudioVideoSwitcher, ISerialComport, ISimpl
    {
        #region ISerialComport Implementation

        /// <summary>
        /// Initializes the driver with a serial COM port.
        /// </summary>
        /// <param name="comPort">The COM port for RS-232 communication.</param>
        public void Initialize(IComPort comPort)
        {
            ConnectionTransport = new CommonSerialComport(comPort)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };

            AudioVideoSwitcherProtocol = new H10X10AVSwitcherProtocol(ConnectionTransport, Id)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            AudioVideoSwitcherProtocol.RxOut += SendRxOut;
            AudioVideoSwitcherProtocol.Initialize(AudioVideoSwitcherData);
        }

        #endregion

        #region ISimpl Implementation

        /// <summary>
        /// Initializes the driver for SIMPL+ testing.
        /// </summary>
        /// <param name="send">The send action delegate for SIMPL transport.</param>
        /// <returns>The SimplTransport instance for testing.</returns>
        public SimplTransport Initialize(Action<string, object[]> send)
        {
            var simplTransport = new SimplTransport { Send = send };
            ConnectionTransport = simplTransport;

            AudioVideoSwitcherProtocol = new H10X10AVSwitcherProtocol(ConnectionTransport, Id)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            AudioVideoSwitcherProtocol.RxOut += SendRxOut;
            AudioVideoSwitcherProtocol.Initialize(AudioVideoSwitcherData);

            return simplTransport;
        }

        #endregion

        #region Connection Properties

        /// <summary>
        /// Gets a value indicating whether this driver supports disconnect operations.
        /// Serial connections do not support disconnect.
        /// </summary>
        public override bool SupportsDisconnect
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this driver supports reconnect operations.
        /// Serial connections do not support reconnect.
        /// </summary>
        public override bool SupportsReconnect
        {
            get { return false; }
        }

        #endregion
    }
}
