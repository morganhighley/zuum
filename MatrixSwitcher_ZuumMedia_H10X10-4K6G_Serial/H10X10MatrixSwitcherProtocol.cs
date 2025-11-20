namespace Crestron.RAD.Drivers.MatrixSwitchers.ZuumMedia
{
    using System;
    using System.Text;
    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.CableBox;
    using Crestron.SimplSharp;

    /// <summary>
    /// Protocol handler for H10X10-4K6G Matrix Switcher
    /// Manages serial communication, command formatting, and response parsing
    /// </summary>
    public class H10X10MatrixSwitcherProtocol : ACableBoxProtocol
    {
        #region Constants

        private const string COMMAND_DELIMITER = "\r\n";
        private const string RESPONSE_DELIMITER = "\r\n";
        private const char SPACE = ' ';

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the H10X10MatrixSwitcherProtocol class
        /// </summary>
        /// <param name="transportDriver">Serial transport driver</param>
        /// <param name="id">Device ID</param>
        public H10X10MatrixSwitcherProtocol(ISerialTransport transportDriver, byte id)
            : base(transportDriver, id)
        {
            CrestronConsole.PrintLine("H10X10MatrixSwitcherProtocol - Constructor");

            // Create response validator
            ResponseValidation = new H10X10ResponseValidator(ValidatedData);

            // Define polling sequence when device powers on
            ValidatedData.PowerOnPollingSequence = new[]
            {
                StandardCommandsEnum.PowerPoll
            };

            CrestronConsole.PrintLine("H10X10MatrixSwitcherProtocol - Constructor complete");
        }

        #endregion

        #region Command Preparation

        /// <summary>
        /// Prepares and formats commands before sending to device
        /// Adds space delimiter and CR+LF terminator
        /// </summary>
        /// <param name="commandSet">Command to prepare and send</param>
        /// <returns>True if command was sent successfully</returns>
        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            var key = "H10X10MatrixSwitcherProtocol_PrepareStringThenSend";

            try
            {
                if (commandSet.CommandPrepared)
                {
                    CrestronConsole.PrintLine("{0} - Command already prepared: {1}", key, commandSet.Command);
                    return base.PrepareStringThenSend(commandSet);
                }

                // Get the raw command
                string command = commandSet.Command;

                CrestronConsole.PrintLine("{0} - Original command: [{1}]", key, command);

                // Add CR+LF delimiter
                commandSet.Command = command + COMMAND_DELIMITER;

                CrestronConsole.PrintLine("{0} - Prepared command: [{1}]", key, commandSet.Command);

                return base.PrepareStringThenSend(commandSet);
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("{0} - Exception: {1}", key, ex.Message);
                return false;
            }
        }

        #endregion

        #region Data Handler

        /// <summary>
        /// Handles incoming data from the device
        /// Parses responses and updates device state
        /// </summary>
        /// <param name="rx">Received data string</param>
        public override void DataHandler(string rx)
        {
            var key = "H10X10MatrixSwitcherProtocol_DataHandler";

            try
            {
                base.DataHandler(rx);

                CrestronConsole.PrintLine("{0} - RxData: [{1}]", key, RxData.ToString());

                // Check for complete message (must have delimiter)
                if (!RxData.ToString().Contains(RESPONSE_DELIMITER))
                {
                    CrestronConsole.PrintLine("{0} - Incomplete message, waiting for more data", key);
                    return;
                }

                string response = RxData.ToString().Trim();
                CrestronConsole.PrintLine("{0} - Complete response: [{1}]", key, response);

                // Update connection status on first successful response
                if (!IsConnected)
                {
                    CrestronConsole.PrintLine("{0} - Setting IsConnected = true", key);
                    IsConnected = true;
                    var connectionObj = new Connection { IsConnected = IsConnected };
                    FireEvent(CableBoxStateObjects.Connection, connectionObj);
                }

                // Parse command acknowledgment
                if (response == "OK")
                {
                    CrestronConsole.PrintLine("{0} - Command acknowledged (OK)", key);

                    // Reset error counters on successful command
                    PartialOrUnrecognizedCommand = false;
                    PartialOrUnrecognizedCommandCount = 0;
                    TimeoutCount = 0;
                }
                else if (response == "NG")
                {
                    CrestronConsole.PrintLine("{0} - Command failed (NG)", key);

                    // Increment error counter
                    PartialOrUnrecognizedCommand = true;
                    PartialOrUnrecognizedCommandCount++;
                }
                else
                {
                    CrestronConsole.PrintLine("{0} - Unrecognized response: {1}", key, response);
                }

                // Clear receive buffer
                RxData.Length = 0;
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
    }
}
