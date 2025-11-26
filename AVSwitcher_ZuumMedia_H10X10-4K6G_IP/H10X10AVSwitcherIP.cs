// Zuum Media H10X10-4K6G HDMI Matrix Switcher - IP Driver
// 4K60Hz 10x10 HDMI Matrix Switcher with Audio Extraction
// Crestron Home AV Switcher Driver for 4-Series Processors

namespace AVSwitcher_ZuumMedia_H10X10_4K6G_IP
{
    using System;
    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.AudioVideoSwitcher;
    using Crestron.SimplSharp;

    /// <summary>
    /// Zuum Media H10X10-4K6G HDMI Matrix Switcher TCP/IP Driver.
    /// Inherits from AAudioVideoSwitcher for proper Crestron Home integration as an AV Switcher device type.
    /// </summary>
    public class H10X10AVSwitcherIP : AAudioVideoSwitcher, ITcp
    {
        #region ITcp Implementation

        /// <summary>
        /// Initializes the driver with TCP/IP connection parameters.
        /// </summary>
        /// <param name="ipAddress">The IP address of the matrix switcher.</param>
        /// <param name="port">The TCP port number (default: 47011).</param>
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

            AudioVideoSwitcherProtocol = new H10X10AVSwitcherProtocol(ConnectionTransport, Id)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            AudioVideoSwitcherProtocol.RxOut += SendRxOut;
            AudioVideoSwitcherProtocol.Initialize(AudioVideoSwitcherData);
        }

        #endregion

        #region Connection Properties

        /// <summary>
        /// Gets a value indicating whether this driver supports disconnect operations.
        /// </summary>
        public override bool SupportsDisconnect
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this driver supports reconnect operations.
        /// </summary>
        public override bool SupportsReconnect
        {
            get { return true; }
        }

        #endregion
    }
}
