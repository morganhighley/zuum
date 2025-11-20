namespace Crestron.RAD.Drivers.VideoServers
{
    using System;
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;

    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.VideoServer;
    using Crestron.SimplSharp;

    public enum CecVersion
    {
        NotRequested = 0x00,
        None = 0x01,
        Version1_3a = 0x04,
        Version1_4ab = 0x05,
        Version2_0 = 0x06
        // Reserved = 0x07, 0x40 - 0xFF
    }

    public class CecVideoServerProtocol : AVideoServerProtocol
    {
        internal bool Initialized;
        internal string LogicalAddress;
        internal uint CecVerValue;
        private CecVersion _cecVersionValue { get; set; }

        public CecVideoServerProtocol(ISerialTransport transportDriver)
            : base(transportDriver, 0x00)
        {
            ResponseValidation = new CecVideoServerResponseValidator(Id, ValidatedData);
            LogicalAddress = "\x04";        //default
            Initialized = false;
            PollingInterval = 30000;
        }

        public override void Initialize(object driverData)
        {
            base.Initialize(driverData);
            ValidateResponse = DriverValidateResponse;
        }

        #region Helper Methods

        internal new void Log(string message)
        {
            if (message != null) base.Log(message);
        }

        private void AssignLogicalAddress(string result)
        {
            switch (result)
            {
                case "\x40":
                    {
                        LogicalAddress = "\x04";
                        break;
                    }
                case "\x80":
                    {
                        LogicalAddress = "\x08";
                        break;
                    }
                case "\xB0":
                    {
                        LogicalAddress = "\x0B";
                        break;
                    }
                case "\x4F":
                    {
                        LogicalAddress = "\x04";
                        break;
                    }
                case "\x8F":
                    {
                        LogicalAddress = "\x08";
                        break;
                    }
                case "\xBF":
                    {
                        LogicalAddress = "\x0B";
                        break;
                    }
                case "\x90":
                    {
                        LogicalAddress = "\x09";
                        break;
                    }
                case "\x9F":
                    {
                        LogicalAddress = "\x09";
                        break;
                    }
                default:
                    {
                        Log("No response from device when polled for CEC version");
                        break;
                    }
            }
        }

        protected override void MessageTimedOut(string lastSentCommand)
        {
            if ((TimeOut != 0) && EnableLogging)
            {
                byte[] buf = Encoding.GetBytes(lastSentCommand);

                StringBuilder debugStringBuilder = new StringBuilder();
                debugStringBuilder.Append(" : MessageTimedOut: ");
                debugStringBuilder.Append(LogTxAndRxAsBytes ? BitConverter.ToString(buf).Replace("-", " ") : lastSentCommand);
                Log(debugStringBuilder.ToString());
            }
        }

        #endregion Helper Methods

        #region Commands

        protected override void Poll()
        {
            Transport.Send("\u000F\u0086\u0010\u0000", null);   //command not dependent on the logical address
            CrestronEnvironment.Sleep(1000);

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        Transport.Send("\x04\x9F", null);
                        break;
                    case 1:
                        Transport.Send("\x08\x9F", null);
                        break;
                    case 2:
                        Transport.Send("\x0B\x9F", null);
                        break;
                    case 3:
                        Transport.Send("\x09\x9F", null);
                        break;
                    default:
                        break;
                }
                CrestronEnvironment.Sleep(1000);
            }
        }

        public override void PowerOn()
        {
            Transport.Send("\u000F\u0086\u0010\u0000", null);   //command not dependent on the logical address
        }

        #region Media Transport

        public override void Play()
        {
            string cmd = LogicalAddress + "\x44\x44";
            string release = LogicalAddress + "\x45";
            Transport.Send(cmd, null);
            Transport.Send(release, null);
        }

        public override void Pause()
        {
            string cmd = LogicalAddress + "\x44\x46";
            string release = LogicalAddress + "\x45";
            Transport.Send(cmd, null);
            Transport.Send(release, null);
        }

        public override void ForwardScan()
        {
            string cmd = LogicalAddress + "\x44\x49";
            string release = LogicalAddress + "\x45";
            Transport.Send(cmd, null);
            Transport.Send(release, null);
        }

        public override void ReverseScan()
        {
            string cmd = LogicalAddress + "\x44\x48";
            string release = LogicalAddress + "\x45";
            Transport.Send(cmd, null);
            Transport.Send(release, null);
        }

        public override void ForwardSkip()
        {
            string cmd = LogicalAddress + "\x44\x4B";
            string release = LogicalAddress + "\x45";
            Transport.Send(cmd, null);
            Transport.Send(release, null);
        }

        public override void ReverseSkip()
        {
            string cmd = LogicalAddress + "\x44\x4C";
            string release = LogicalAddress + "\x45";
            Transport.Send(cmd, null);
            Transport.Send(release, null);
        }

        #endregion Media Transport

        #region Navigation

        public override void PressArrowKey(ArrowDirections direction)
        {
            string cmd;
            string release;
            switch (direction)
            {
                case ArrowDirections.Up:
                    cmd = LogicalAddress + "\x44\x01";
                    release = LogicalAddress + "45";
                    Transport.Send(cmd, null);
                    Transport.Send(release, null);
                    break;
                case ArrowDirections.Down:
                    cmd = LogicalAddress + "\x44\x02";
                    release = LogicalAddress + "45";
                    Transport.Send(cmd, null);
                    Transport.Send(release, null);
                    break;
                case ArrowDirections.Left:
                    cmd = LogicalAddress + "\x44\x03";
                    release = LogicalAddress + "45";
                    Transport.Send(cmd, null);
                    Transport.Send(release, null);
                    break;
                case ArrowDirections.Right:
                    cmd = LogicalAddress + "\x44\x04";
                    release = LogicalAddress + "45";
                    Transport.Send(cmd, null);
                    Transport.Send(release, null);
                    break;
            }
        }

        public override void Select()
        {
            string cmd = LogicalAddress + "\x44\x00";
            string release = LogicalAddress + "\x45";
            Transport.Send(cmd, null);
            Transport.Send(release, null);
        }

        public override void Menu()
        {
            string cmd = LogicalAddress + "\x44\x09";
            string release = LogicalAddress + "\x45";
            Transport.Send(cmd, null);
            Transport.Send(release, null);
        }

        #endregion Navigation

        #endregion Commands

        #region ProcessResponses

        private ValidatedRxData ProcessCECVersionResponse(ValidatedRxData validatedData, string response)
        {
            if (response.Contains("\x04"))
            {
                _cecVersionValue = CecVersion.Version1_3a;
                CecVerValue = (uint)_cecVersionValue;
            }
            else if (response.Contains("\x05"))
            {
                _cecVersionValue = CecVersion.Version1_4ab;
                CecVerValue = (uint)_cecVersionValue;
            }
            else if (response.Contains("\x06"))
            {
                _cecVersionValue = CecVersion.Version2_0;
                CecVerValue = (uint)_cecVersionValue;
            }
            else
            {
                _cecVersionValue = CecVersion.None;
                CecVerValue = (uint)_cecVersionValue;
            }
            Initialized = true;
            validatedData.Data = response;
            validatedData.CommandGroup = CommonCommandGroupType.Unknown;
            validatedData.Ready = true;

            return validatedData;
        }

        #endregion ProcessResponses

        #region Validate Response

        public ValidatedRxData DriverValidateResponse(string response, CommonCommandGroupType commandGroup)
        {
            ValidatedRxData validatedData = new ValidatedRxData(false, string.Empty);

            if (response.Equals("\x00") || response.Equals("\x04") || response.Equals("\x05") || response.Equals("\x06") || response.Equals("\x10") ||
                response.Equals("\x36") || response.Equals("\x41") || response.Equals("\x56") || response.Equals("\x66") || response.Equals("\x6D") ||
                response.Equals("\x76") || response.Equals("\x83") || response.Equals("\x85") || response.Equals("\x87") || response.Equals("\x8C") ||
                response.Equals("\xA6"))
            {
                //CrestronConsole.PrintLine("ignoring device discovery poll request from video server");
                validatedData.Ignore = true;
            }
            else if (response.Contains("\x40\x8F") || response.Contains("\x80\x8F") || response.Contains("\x90\x8F") || response.Contains("B0\x8F"))
            {
                //CrestronConsole.PrintLine("ignoring device power poll request from video server");

                string result = response.Substring(0, 1);
                AssignLogicalAddress(result);

                validatedData.Ignore = true;
            }
            else if (response.Contains("\x40\x9F") || response.Contains("\x80\x9F") || response.Contains("\x90\x9F") || response.Contains("\xB0\x9F"))
            {
                //CrestronConsole.PrintLine("ignoring give cec version request response from video server");
                validatedData.Ignore = true;
            }
            else if (response.Contains("\x40\x8C") || response.Contains("\x80\x8C") || response.Contains("\x90\x8C") || response.Contains("\xB0\x8C"))
            {
                //CrestronConsole.PrintLine("ignoring give device vendor id request response from video server");
                validatedData.Ignore = true;
            }
            else if (response.Contains("\x8F\x87\x00\x0C\xE7"))
            {
                //CrestronConsole.PrintLine("ignoring device vendor id response from video server");
                validatedData.Ignore = true;
            }
            else if (response.Contains("\x80\x47\x41\x6D\x61\x7A\x6F\x6E\x20\x46\x69\x72\x65\x54\x56"))
            {
                //CrestronConsole.PrintLine("ignoring OSD Amazon FireTV response from video server");
                validatedData.Ignore = true;
            }
            else if (response.Contains("\x4E\x05") || response.Contains("\x8E\x05") || response.Contains("\x9E\x05") || response.Contains("\xBE\x05"))
            {
                //CrestronConsole.PrintLine("ignoring device tuner step increment request from video server");
                validatedData.Ignore = true;
            }
            else if (response.Contains("\x36\x06") || response.Contains("\x66\x06") || response.Contains("\x76\x06") || response.Contains("\xA6\x06"))
            {
                //CrestronConsole.PrintLine("ignoring device tuner step decrement request from video server");
                validatedData.Ignore = true;
            }
            else if (response.Contains("\x84\x10\x00"))
            {
                //CrestronConsole.PrintLine("ignoring unimplemented response");
                //it should be \x4F\x84\x10\x00 but the language poll is \x4F and with Byte By Byte it is impossible to know if it is this or that so this catches
                //catches the message fragment and throws it out to avoid issues.
                validatedData.Ignore = true;
            }
            else if (response.Contains("\x40\x9E") || response.Contains("\x80\x9E") || response.Contains("\xB0\x9E"))
            {
                //CrestronConsole.PrintLine("checking for possible cec version response");

                string result = response.Substring(0, 1);
                AssignLogicalAddress(result);

                validatedData = ProcessCECVersionResponse(validatedData, response);
            }
            else if (response.Contains("\x4E\x06") || response.Contains("\x8E\x06") || response.Contains("\x9E\x06") || response.Contains("\xBE\x06"))
            {
                //CrestronConsole.PrintLine("Ignoring video server tune request");

                string result = response.Substring(0, 1);
                //AssignLogicalAddress(result);

                validatedData.Ignore = true;
            }
            else if (response.Contains("\x40\x90") || response.Contains("\x80\x90") || response.Contains("\xB0\x90") || response.Contains("\x90\x90") ||
                     response.Contains("\x4F\x90") || response.Contains("\x8F\x90") || response.Contains("\xBF\x90") || response.Contains("\x9F\x90"))
            {
                //CrestronConsole.PrintLine("In the current catch all branch that exists for now");
                string result = response.Substring(0, 1);
                AssignLogicalAddress(result);
                validatedData.Ignore = true;
            }

            //CrestronConsole.PrintLine("ready = {0}", validatedData.Ready);
            //byte[] commandBytes = Encoding.GetBytes(response);
            //string readableString = BitConverter.ToString(commandBytes);
            //CrestronConsole.PrintLine("Response: {0}", readableString);

            return validatedData;
        }

        #endregion
    }
}
