// Zuum Media H10X10-4K6G HDMI Matrix Switcher - Protocol Handler
// Handles command formatting and response parsing for the H10X10-4K6G protocol

namespace AVSwitcher_ZuumMedia_H10X10_4K6G_IP
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.AudioVideoSwitcher;

    /// <summary>
    /// Protocol handler for the Zuum Media H10X10-4K6G HDMI Matrix Switcher.
    /// Handles command formatting with CR+LF delimiters and response parsing.
    /// </summary>
    public class H10X10AVSwitcherProtocol : AAudioVideoSwitcherProtocol
    {
        #region Constants

        /// <summary>
        /// Command delimiter - Carriage Return + Line Feed.
        /// </summary>
        private const string CommandDelimiter = "\r\n";

        /// <summary>
        /// Response delimiter - Carriage Return + Line Feed.
        /// </summary>
        private const string ResponseDelimiter = "\r\n";

        /// <summary>
        /// Acknowledgment response from device.
        /// </summary>
        private const string AckResponse = "OK";

        /// <summary>
        /// Negative acknowledgment response from device.
        /// </summary>
        private const string NakResponse = "NG";

        /// <summary>
        /// Regex pattern to parse routing status from device.
        /// Format: OUTx INy where x is output number and y is input number.
        /// </summary>
        private static readonly Regex RouteStatusPattern = new Regex(
            @"OUT(\d+)\s+IN(\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #endregion

        #region Fields

        /// <summary>
        /// Buffer for accumulating partial responses.
        /// </summary>
        private readonly StringBuilder _responseBuffer = new StringBuilder();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the H10X10AVSwitcherProtocol class.
        /// </summary>
        /// <param name="transport">The serial transport for communication.</param>
        /// <param name="id">The driver instance ID.</param>
        public H10X10AVSwitcherProtocol(ISerialTransport transport, byte id)
            : base(transport, id)
        {
        }

        #endregion

        #region Command Preparation

        /// <summary>
        /// Prepares the command string by adding the CR+LF delimiter before sending.
        /// </summary>
        /// <param name="commandSet">The command set to prepare and send.</param>
        /// <returns>True if the command was sent successfully; otherwise, false.</returns>
        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            if (!commandSet.CommandPrepared)
            {
                // Add CR+LF delimiter to the command
                commandSet.Command = string.Format("{0}{1}", commandSet.Command, CommandDelimiter);
                commandSet.CommandPrepared = true;
            }

            return base.PrepareStringThenSend(commandSet);
        }

        #endregion

        #region Response Handling

        /// <summary>
        /// Handles incoming data from the device, splitting multi-line responses.
        /// </summary>
        /// <param name="rx">The received data string.</param>
        public override void DataHandler(string rx)
        {
            // Accumulate data in the buffer
            _responseBuffer.Append(rx);
            string buffer = _responseBuffer.ToString();

            // Check for complete responses (ending with delimiter)
            int delimiterIndex;
            while ((delimiterIndex = buffer.IndexOf(ResponseDelimiter, StringComparison.Ordinal)) >= 0)
            {
                // Extract complete response
                string response = buffer.Substring(0, delimiterIndex);
                buffer = buffer.Substring(delimiterIndex + ResponseDelimiter.Length);

                // Process the complete response if not empty
                if (!string.IsNullOrEmpty(response.Trim()))
                {
                    base.DataHandler(response + ResponseDelimiter);
                }
            }

            // Update buffer with remaining partial data
            _responseBuffer.Clear();
            _responseBuffer.Append(buffer);
        }

        /// <summary>
        /// Deconstructs the switcher route feedback and updates extender states.
        /// </summary>
        /// <param name="response">The route feedback response string.</param>
        protected override void DeConstructSwitcherRoute(string response)
        {
            // Parse routing status responses
            // Expected format: OUT1 IN5 (output 1 is routed to input 5)
            var matches = RouteStatusPattern.Matches(response);

            foreach (Match match in matches)
            {
                if (match.Success && match.Groups.Count >= 3)
                {
                    string outputNum = match.Groups[1].Value;
                    string inputNum = match.Groups[2].Value;

                    // Get the extender objects by their API identifiers
                    var outputExtender = GetExtenderByApiIdentifier(outputNum);
                    var inputExtender = inputNum != "0" ? GetExtenderByApiIdentifier(inputNum) : null;

                    if (outputExtender != null)
                    {
                        // Update the output extender with the current source routed to it
                        outputExtender.VideoSourceExtenderId = inputExtender?.Id;
                        outputExtender.AudioSourceExtenderId = inputExtender?.Id;
                    }
                }
            }
        }

        /// <summary>
        /// Deconstructs the switcher power feedback.
        /// </summary>
        /// <param name="response">The power feedback response string.</param>
        protected override void DeConstructSwitcherPower(string response)
        {
            // Handle power state responses
            // The Zuum matrix doesn't have a traditional power state - it's always on when connected
            // However, we handle this for completeness
            base.DeConstructSwitcherPower(response);
        }

        #endregion
    }
}
