namespace Crestron.RAD.Drivers.MatrixSwitchers.ZuumMedia
{
    using System;
    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.CableBox;
    using Crestron.RAD.ProTransports;
    using Crestron.SimplSharp;

    /// <summary>
    /// Zuum Media H10X10-4K6G 10x10 HDMI Matrix Switcher Driver
    /// Supports RS-232 serial communication at 9600 baud, 8-N-1
    /// </summary>
    public class H10X10MatrixSwitcherSerial : ACableBox, ISerialComport, ISimpl
    {
        #region Fields

        private SimplTransport _transport;

        #endregion

        #region ISerialComport Members

        /// <summary>
        /// Initializes the driver with a physical COM port (Crestron hardware)
        /// </summary>
        /// <param name="comPort">COM port from Crestron processor</param>
        public void Initialize(IComPort comPort)
        {
            var key = "H10X10MatrixSwitcherSerial_Initialize";
            try
            {
                CrestronConsole.PrintLine("{0} - Initialize: Start", key);

                // Create RS-232 transport layer
                ConnectionTransport = new CommonSerialComport(comPort)
                {
                    EnableLogging = InternalEnableLogging,
                    CustomLogger = InternalCustomLogger,
                    EnableRxDebug = InternalEnableRxDebug,
                    EnableTxDebug = InternalEnableTxDebug
                };

                CrestronConsole.PrintLine("{0} - Transport created", key);

                // Create protocol handler
                CableBoxProtocol = new H10X10MatrixSwitcherProtocol(ConnectionTransport, Id)
                {
                    EnableLogging = InternalEnableLogging,
                    CustomLogger = InternalCustomLogger
                };

                CrestronConsole.PrintLine("{0} - Protocol created", key);

                // Wire up event handlers
                CableBoxProtocol.StateChange += StateChange;
                CableBoxProtocol.RxOut += SendRxOut;

                CrestronConsole.PrintLine("{0} - Event handlers wired", key);

                // Initialize protocol with device data from JSON manifest
                CableBoxProtocol.Initialize(CableBoxData);

                CrestronConsole.PrintLine("{0} - Complete", key);
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("{0} - Exception: {1}", key, ex.Message);
                if (ex.InnerException != null)
                {
                    CrestronConsole.PrintLine("{0} - Inner Exception: {1}", key, ex.InnerException.Message);
                }
            }
        }

        #endregion

        #region ISimpl Members

        /// <summary>
        /// Initializes the driver for SIMPL+ testing/simulation
        /// </summary>
        /// <param name="send">Callback function for sending data</param>
        /// <returns>SimplTransport instance</returns>
        public SimplTransport Initialize(Action<string, object[]> send)
        {
            var key = "H10X10MatrixSwitcherSerial_Initialize(SIMPL)";
            try
            {
                CrestronConsole.PrintLine("{0} - Initialize: Start", key);

                _transport = new SimplTransport { Send = send };
                ConnectionTransport = _transport;

                CableBoxProtocol = new H10X10MatrixSwitcherProtocol(ConnectionTransport, Id)
                {
                    EnableLogging = InternalEnableLogging,
                    CustomLogger = InternalCustomLogger
                };

                CableBoxProtocol.StateChange += StateChange;
                CableBoxProtocol.RxOut += SendRxOut;
                CableBoxProtocol.Initialize(CableBoxData);

                CrestronConsole.PrintLine("{0} - Complete", key);

                return _transport;
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("{0} - Exception: {1}", key, ex.Message);
                throw;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Indicates whether the driver supports disconnect functionality
        /// </summary>
        public override bool SupportsDisconnect
        {
            get { return false; }
        }

        /// <summary>
        /// Indicates whether the driver supports reconnect functionality
        /// </summary>
        public override bool SupportsReconnect
        {
            get { return false; }
        }

        #endregion
    }
}
