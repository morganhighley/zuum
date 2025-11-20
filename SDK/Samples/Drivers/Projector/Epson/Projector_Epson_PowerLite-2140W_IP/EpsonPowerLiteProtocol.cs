// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EpsonPowerLiteProtocol.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Crestron.RAD.Common.Helpers;
using Crestron.SimplSharp;

namespace Crestron.RAD.Drivers.Displays
{
    using System;

    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.Display;

    public class EpsonPowerLiteProtocol : ADisplayProtocol
    {
        private const string _tcpClientHandshakeWithoutPassword = "ESC/VP.net\u0010\u0003\u0000\u0000\u0000\u0000";
        private const string _powerOnResponse = "01";
        private const string _warmingUpResponse = "02";
        private const string _coolDownResponse = "03";
        private const string _standbyResponse = "04";

        private Stopwatch _powerWaitPeriodStopwatch = new Stopwatch();
        internal bool _passwordRequired;
        internal bool _readyForCommands;
        internal string _tcpClientHandshakeWithPassword;
        private System.Collections.Generic.Queue<CommandSet> _commandSetQueue;
        private CCriticalSection _commandSetQueueLock;

        public EpsonPowerLiteProtocol(ISerialTransport transportDriver, byte id, EpsonPowerLiteTcp driver)
            : base(transportDriver, id)
        {
            ResponseValidation = new ResponseValidator(Id, ValidatedData, driver, this);
            ValidatedData.PowerOnPollingSequence = new[] 
            { 
                StandardCommandsEnum.PowerPoll, 
                StandardCommandsEnum.VideoMutePoll, 
                StandardCommandsEnum.LampHoursPoll, 
                StandardCommandsEnum.VolumePoll,
                StandardCommandsEnum.InputPoll
            };

            _tcpClientHandshakeWithPassword = string.Empty;
            _passwordRequired = false;
            _readyForCommands = false;
            _commandSetQueue = new System.Collections.Generic.Queue<CommandSet>();
            _commandSetQueueLock = new CCriticalSection();
        }

        protected override void ConnectionChanged(bool connection)
        {
            // TCP/IP require a handshake before the server accepts any commands
            if (connection)
            {
                if (_passwordRequired)
                {
                    Transport.Send(_tcpClientHandshakeWithPassword, null);
                }
                else
                {
                    Transport.Send(_tcpClientHandshakeWithoutPassword, null);
                }
            }

            base.ConnectionChanged(connection);
        }

        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            if (!commandSet.CommandPrepared)
            {
                // Append <CR> to all commands being sent out
                commandSet.Command += "\u000D";
                commandSet.CommandPrepared = true;
            }

            if (_readyForCommands)
            {
                return base.PrepareStringThenSend(commandSet);
            }
            else
            {
                if (!commandSet.IsPollingCommand)
                {
                    EnqueueCommandSet(commandSet);
                }
                return false;
            }
        }

        private void EnqueueCommandSet(CommandSet commandSet)
        {
            if (commandSet != null)
            {
                try
                {
                    _commandSetQueueLock.Enter();
                    _commandSetQueue.Enqueue(commandSet);
                }
                finally
                {
                    _commandSetQueueLock.Leave();
                }
            }
        }

        internal void DequeueCommandSets()
        {
            try
            {
                _commandSetQueueLock.Enter();
                while (_commandSetQueue.Count > 0)
                {
                    SendCommand(_commandSetQueue.Dequeue());
                }
            }
            finally
            {
                _commandSetQueueLock.Leave();
            }
        }

        // Display will tell us when it is cooling down or warming up. 
        // Treat these as Power On and Power Off feedback
        protected override void DeConstructPower(string response)
        {
            switch (response)
            {
                case _warmingUpResponse:
                    response = HandleWarmupResponse();
                    break;
                case _coolDownResponse:
                    response = HandleCoolDownResponse();
                    break;
                case _standbyResponse:
                    response = HandleStandbyResponse();
                    break;
                case _powerOnResponse:
                    response = HandlePowerOnResponse();
                    break;
            }
            DiagnosticLogging();
            base.DeConstructPower(response);
        }

        private string HandleWarmupResponse()
        {
            Log("DeconstructPower:Epson is Warming up");
            return ValidatedData.PowerFeedback
                .Feedback[StandardFeedback.PowerStatesFeedback.On];
        }

        private string HandleCoolDownResponse()
        {
            Log("DeconstructPower:Epson is Cooling down");
            return ValidatedData.PowerFeedback
                .Feedback[StandardFeedback.PowerStatesFeedback.Off];
        }

        private string HandleStandbyResponse()
        {
            Log("DeconstructPower:Epson is in Standby, Powered Off");
            return ValidatedData.PowerFeedback
                .Feedback[StandardFeedback.PowerStatesFeedback.Off];
        }

        private string HandlePowerOnResponse()
        {
            Log("DeconstructPower:Epson is Powered On");
            Log("DeconstructPower:Epson has Warmed up.");
            return ValidatedData.PowerFeedback
                .Feedback[StandardFeedback.PowerStatesFeedback.On];
        }

        private void DiagnosticLogging()
        {
            if (WarmingUp || CoolingDown)
            {
                if (!_powerWaitPeriodStopwatch.IsRunning)
                {
                    Log(string.Format("Epson:Diagnostic stopwatch started to measure power wait period.",
                        _powerWaitPeriodStopwatch.Elapsed.TotalSeconds));
                    _powerWaitPeriodStopwatch.Start();
                }
            }
            else
            {
                if (_powerWaitPeriodStopwatch.IsRunning)
                {
                    Log(string.Format("Epson:{0}seconds elapsed for power wait period.",
                        _powerWaitPeriodStopwatch.Elapsed.TotalSeconds));
                    _powerWaitPeriodStopwatch.Reset();
                }
            }
        }

        internal void AuthenticationEvent(bool isAuthenticated)
        {
            FireEvent(DisplayStateObjects.Authentication, isAuthenticated);
        }
    }


    public class ResponseValidator : ResponseValidation
    {
        private const int _locationOfStatusCodeInEscVpNetHeader = 14;
        private const string _escVpNetHeader = "ESC/VP.net\u0010\u0003\u0000\u0000";
        private const string _loginCommand = "ESC/VP.net\u0010\u0003\u0000\u0000\u0000\u0001\u0001\u0001";
        private const int _volumeMultiplier = 12;
        private const int _passwordTotalLength = 16;
        private const char _passwordPadCharacter = '\x00';

        private EpsonPowerLiteTcp _driver;
        private EpsonPowerLiteProtocol _protocol;

        public ResponseValidator(byte id, DataValidation dataValidation, EpsonPowerLiteTcp driver, EpsonPowerLiteProtocol protocol)
            : base(id, dataValidation)
        {
            Id = id;
            DataValidation = dataValidation;

            _driver = driver;
            _protocol = protocol;
        }

        public override ValidatedRxData ValidateResponse(string response, CommonCommandGroupType commandGroup)
        {
            ValidatedRxData validatedData = new ValidatedRxData(false, string.Empty);
            // Display will respond with single ":" every time power is switched off
            // Ignore it
            // Added check for len of 1 to ensure that this response only gets processed once.
            if (response == ":" && response.Length == 1)
            {
                validatedData.CommandGroup = CommonCommandGroupType.AckNak;
                validatedData.Data = DataValidation.AckDefinition;
                validatedData.Ready = true;
                return validatedData;
            }

            // TCP/IP Connections require a handshake when the connection starts
            // Either the connect attempt worked (no authentication), or an error code was received
            // and a password must be sent
            if (response.StartsWith(_escVpNetHeader) &&
                response.Length > _escVpNetHeader.Length + 1)
            {
                var statusCode = response[_locationOfStatusCodeInEscVpNetHeader];

                switch (statusCode)
                {
                    case '\x20':
                        // This code is when a password is not required
                        _protocol._readyForCommands = true;

                        if (_protocol._passwordRequired)
                        {
                            _protocol.AuthenticationEvent(true);
                            _protocol.DequeueCommandSets();
                        }
                        break;
                    default:
                        _protocol._passwordRequired = true;
                        var password = string.Empty;

                        if (!string.IsNullOrEmpty(_driver._passwordKey))
                        {
                            var dataStore = new Crestron.RAD.Common.CrestronDataStoreWrapper();
                            object pass = default(object);
                            dataStore.GetLocalValue(_driver._passwordKey, typeof(string), out pass);
                            password = pass == null ? string.Empty : (string)pass;
                        }
                        else
                        {
                            password = _driver._password == null ? string.Empty : _driver._password;
                        }
                        _protocol._tcpClientHandshakeWithPassword = string.Format("{0}{1}",
                            _loginCommand, password.PadRight(_passwordTotalLength, _passwordPadCharacter));
                        _protocol._readyForCommands = false;
                        _protocol.AuthenticationEvent(false);
                        break;
                }

                validatedData.CommandGroup = CommonCommandGroupType.AckNak;
                validatedData.Data = DataValidation.AckDefinition;
                validatedData.Ready = true;
                return validatedData;
            }

            // 2 = <CR> + ":"
            if (response.EndsWith(":") && response.Length > 2)
            {
                _protocol._readyForCommands = true;
                _protocol.DequeueCommandSets();

                // Ignore the IMEVENT packets
                if (response.Contains("IMEVENT") || response.Contains(DataValidation.NakDefinition))
                {
                    validatedData.Data = DataValidation.AckDefinition;
                    validatedData.Ready = true;
                    return validatedData;
                }

                // Remove <CR> and ":"
                response = response.Substring(0, response.Length - 2);

                // Handle volume manually
                if (response.Contains(DataValidation.VolumeFeedback.GroupHeader))
                {
                    validatedData.Data = RemoveHeader(response, DataValidation.VolumeFeedback.GroupHeader);
                    try
                    {
                        int volume = Convert.ToInt32(validatedData.Data);
                        volume /= _volumeMultiplier;
                        validatedData.Data = volume.ToString();
                        validatedData.CommandGroup = CommonCommandGroupType.Volume;
                        validatedData.Ready = true;
                    }
                    catch
                    {
                        validatedData.Data = DataValidation.NakDefinition;
                        validatedData.CommandGroup = CommonCommandGroupType.AckNak;
                        validatedData.Ready = true;
                    }

                    return validatedData;
                }

                // Everything else can be handled by the base
                return base.ValidateResponse(response, commandGroup);
            }

            return new ValidatedRxData(false, string.Empty);
        }
    }
}

