    
using System.Linq;

namespace Crestron.RAD.Drivers.Displays
{
    using System;
    using System.Globalization;

    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Helpers;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.Display;
    using Crestron.SimplSharp;
    using Crestron.RAD.Common;


    public class SonyXBRSeriesProtocol: ADisplayProtocol
    {
        public ushort ChecksumStartByte = 1;
        public bool StandByOnCommand = false;

        private bool _muteDebounced = false;
        private uint _muteTicks = 0;
        private int _lastMuteBeginTick = 0;
        private uint _muteDebounceTicks = 500;

        private bool _rampingValues = false;
        private int _currentVolume;
        private uint _volumeTicks = 0;
        private int _lastRampBeginTick = 0;
        private int _volumeRepeatRate = 450;
        private const int _ignoredRampFeedbackTicks = 2000;
        internal bool VolumePressed = false;
        private AbsoluteValidator _responseValidatorRef;


        private string StandbyCmd = "\x8C\x00\x01\x02\x01\x90"; //checksum included in string

        public SonyXBRSeriesProtocol(ISerialTransport transportDriver, byte id)
            : base(transportDriver, id)
        {
            //_responseValidatorRef = new AbsoluteValidator(Id, ValidatedData, this);
            ResponseValidation = new AbsoluteValidator(Id, ValidatedData, this);
            ValidatedData.PowerOnPollingSequence = new[] 
            { 
                StandardCommandsEnum.PowerPoll, 
                StandardCommandsEnum.InputPoll, 
                StandardCommandsEnum.VolumePoll, 
                StandardCommandsEnum.MutePoll
            };
           
        }

        internal void SendCustomPriorityCommand(string name, string message, CommonCommandGroupType groupType,
            CommandPriority priority, StandardCommandsEnum commandEnum)
        {
            CommandSet command = new CommandSet(name, message, groupType,
                null, false, priority, commandEnum);
            SendCommand(command);
        }

        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            //sends standby enable command once; Required to power on tv when off
            if (!StandByOnCommand && PowerIsOn)
            { 
                Transport.Send(StandbyCmd,null);
                StandByOnCommand = !StandByOnCommand;
                return true;
            }

            else
            {
                commandSet.Command = string.Format("{0}{1}", commandSet.Command, CalculateChecksum(commandSet.Command));
                return base.PrepareStringThenSend(commandSet);
            }
        }
        private void UpdateResponseValidationState(bool state)
        {
            if (ResponseValidation.GetType() == typeof(AbsoluteValidator))
            {
                (ResponseValidation as AbsoluteValidator).PowerOffIssued = state;
            }
        }

        //check sum = total sum of bytes in command sent
        //if over 255(1 byte), last 1 byte of data is used
        private string CalculateChecksum(string data)
        {
            byte chksum = 0x00;

            var bytes = Encoding.GetBytes(data);

            for (var i = 0; i < bytes.Length; i++)
            {
                chksum += bytes[i];
            }

            chksum &= 0xFF;

            return Encoding.GetString(new byte[] { chksum }, 0, 1);
        }

        private byte[] CalculateAndAppendChecksum(string command)
        {
            var commandChars = command.ToCharArray();
            var commandBytes = new byte[commandChars.Length + 1];

            for (var i = 0; i < commandChars.Length; i++)
                commandBytes[i] = Convert.ToByte(commandChars[i]);

            for (var i = ChecksumStartByte - 1; i < commandChars.Length; i++)
                commandBytes[commandBytes.Length - 1] += commandBytes[i];

            return commandBytes;
        }

        private void StartMuteDebounce()
        {
            _lastMuteBeginTick = CrestronEnvironment.TickCount;
            _muteDebounced = true;
        }

        private void PerformMuteCheck()
        {
            if (Math.Abs(CrestronEnvironment.TickCount - _lastMuteBeginTick) > _muteDebounceTicks)
            {
                _muteDebounced = false;
            }
        }

        private void StartRampingValue()
        {
            _lastRampBeginTick = CrestronEnvironment.TickCount;
            _rampingValues = true;
        }

        private void PerformRampingCheck()
        {
            if (Math.Abs(CrestronEnvironment.TickCount - _lastRampBeginTick) > _ignoredRampFeedbackTicks)
            {
                _rampingValues = false;
            }
        }

        /******
        public override void IncrementVolume()
        {
            CrestronConsole.PrintLine("In IncrementVolume _currentVolume={0}", _currentVolume);
            if (_currentVolume < 100)
            {
                _currentVolume++;
                SetVolume((uint)_currentVolume);
            }
            else
            {
                _currentVolume = 100;
            }
        }
        ******/

        public override void IncrementVolume()
        {
                if (CheckIfCommandExists(StandardCommandsEnum.Vol))
                {
                    if (RampingVolumeIs >= 100)
                    {
                        SetVolume(RampingVolumeIs);
                    }
                    else
                    {
                        SetVolume(RampingVolumeIs + 1);
                        //Allow for VolumeTick or ReleaseVolume incrementing UnscaledRampingVolumeIs by 1
                        //by decrementing here ... can't override VolumeTick.
                        //RAD Testtool must be accessing this variable
                        UnscaledRampingVolumeIs--;
                    }
                }
    
        }




        /*********
        public override void DecrementVolume()
        {
            CrestronConsole.PrintLine("In DecrementVolume _currentVolume={0}", _currentVolume);
            if (_currentVolume > 0)
            {
                _currentVolume--;
                SetVolume((uint)_currentVolume);
            }
            else
            {
                _currentVolume = 0;
            }
        }
        ***********/

        public override void DecrementVolume()
        {
            if (CheckIfCommandExists(StandardCommandsEnum.Vol))
                {
                    if (RampingVolumeIs == 0)
                    {
                        SetVolume(RampingVolumeIs);
                    }
                    else
                    {
                        SetVolume(RampingVolumeIs - 1);
                        //Allow for VolumeTick or ReleaseVolume decrementing UnscaledRampingVolumeIs by 1
                        //by incrementing here ... can't override VolumeTick.
                        //RAD Testtool must be accessing this variable
                        UnscaledRampingVolumeIs++;
                    }
                }
  
        }




        /********
        public override void PressVolumeUp()
        {
            CrestronConsole.PrintLine("In override PressVolumeUp");
            RampingVolumeState = RampingVolumeState.Up;
            IsRamping = true;

            if (VolumeRampTimer == null)
            {
                VolumeRampTimer = new CTimer(VolumeTick, null, 0, _volumeRepeatRate);
            }
        }

        public override void PressVolumeDown()
        {
            CrestronConsole.PrintLine("In override PressVolumeDown");
            RampingVolumeState = RampingVolumeState.Down;
            IsRamping = true;

            if (VolumeRampTimer == null)
            {
                VolumeRampTimer = new CTimer(VolumeTick, null, 0, _volumeRepeatRate);
            }
        }

        public override void ReleaseVolume()
        {
            CrestronConsole.PrintLine("In override Releasevolume IsRamping={0} RampingVolumeState={1} _volumeTicks={2}", IsRamping, RampingVolumeState, _volumeTicks);
            if (VolumeRampTimer != null)
            {
                VolumeRampTimer.Stop();
                VolumeRampTimer = null;

                if (IsRamping)
                {
                    IsRamping = false;

                    if (_volumeTicks == 0)
                    {
                        if (RampingVolumeState == RampingVolumeState.Up)
                        {
                            IncrementVolume();
                        }
                        else if (RampingVolumeState == RampingVolumeState.Down)
                        {
                            DecrementVolume();
                        }
                    }
                    else
                    {
                        _volumeTicks = 0;
                    }
                }
            }
        }

        protected new void VolumeTick(object obj)
        {
            CrestronConsole.PrintLine("In new VolumeTick()");
            if (IsRamping)
            {
                _volumeTicks++;
                if (RampingVolumeState == RampingVolumeState.Up)
                {
                    IncrementVolume();
                }
                else
                {
                    DecrementVolume();
                }
            }
            else if (VolumeRampTimer != null)
            {
                VolumeRampTimer = null;
            }
        }
        ********/
        

        protected override void DeConstructVolume(string response)
        {
            const string header = "\u0003\u0001";
            response = response.Replace(header, "");
            try 
            {
                //trimming header leaves an empty string for zero volume which throws an exception
                if (string.IsNullOrEmpty(response))
                {
                   base.DeConstructVolume("0");
                }
                else
                {
                   response = BitConverter.ToString(Encoding.GetBytes(response));     //get hex value from bytes
                   int value = Convert.ToInt32(response, 16);                         //converts hex string to decimal equivalent
                   if (value > 100)
                   {
                       return;
                   }

                   base.DeConstructVolume(value.ToString());
                }
           }
           catch(Exception e)
           {
               Log(String.Format("DeConstructVolume: Expected VolumePercent Feedback is not a valid numerical value. Reason={0}", e.Message));
                return;
           }  
        }

        #region Commands

        public override void MuteOff()
        {
            if (MuteIsOn)
            {
                if (_muteDebounced)
                {
                    PerformMuteCheck();
                }

                if (!_muteDebounced)
                {
                    StartMuteDebounce();
                    base.MuteOff();
                    SendCustomPriorityCommand("MutePoll", "\u0083\u0000\u0006\u00FF\u00FF\u0087", CommonCommandGroupType.Mute,
                        CommandPriority.Special, StandardCommandsEnum.MutePoll);
                }
            }
        }

        public override void MuteOn()
        {
            if (!MuteIsOn)
            {
                if (_muteDebounced)
                {
                    PerformMuteCheck();
                }

                if (!_muteDebounced)
                {
                    StartMuteDebounce();
                    base.MuteOn();
                    SendCustomPriorityCommand("MutePoll", "\u0083\u0000\u0006\u00FF\u00FF\u0087", CommonCommandGroupType.Mute,
                        CommandPriority.Special, StandardCommandsEnum.MutePoll);
                }
            }
        }

        /*********
        public override void SetVolume(uint volume)
        {
            CrestronConsole.PrintLine("In override SetVolume 1 IsRamping={0} RampingVolumeState={1} volume={2} UnscaledRampingVolumeIs={3}", IsRamping, RampingVolumeState, volume, UnscaledRampingVolumeIs);
            _currentVolume = (int)volume;

            volume = VolumeHelper.ScaleVolume(
                    new VolumeDetail(volume, MinVolume, MaxVolume,
                        IsRamping, RampingVolumeState, UnscaledRampingVolumeIs));
            CrestronConsole.PrintLine("In override SetVolume 2 IsRamping={0} RampingVolumeState={1} volume={2} UnscaledRampingVolumeIs={3}", IsRamping, RampingVolumeState, volume, UnscaledRampingVolumeIs);
            Commands command = DisplayData.CrestronSerialDeviceApi.Api.StandardCommands[StandardCommandsEnum.Vol];
            string formattedVolume = Convert.ToString(volume);
            CrestronConsole.PrintLine("In override SetVolume 3 IsRamping={0} RampingVolumeState={1} formattedVolume={2}", IsRamping, RampingVolumeState, formattedVolume);

            var volumeParameter = ParameterHelper.GetFirstValidParameter(command);
            formattedVolume = ParameterHelper.FormatValue(formattedVolume, volumeParameter);
            string volumeParameterTag = "!$[" + volumeParameter.Id + "]";
            string modifiedCommand = ParameterHelper.ReplaceParameter(command.Command, volumeParameterTag, formattedVolume);
            CrestronConsole.PrintLine("In override SetVolume 4 IsRamping={0} RampingVolumeState={1} modifiedCommand={2}", IsRamping, RampingVolumeState, modifiedCommand);

            CommandSet volumeCommand = BuildCommand(StandardCommandsEnum.Vol, CommonCommandGroupType.Volume,
                CommandPriority.Low, "Volume " + Convert.ToString(volume), modifiedCommand);

            CrestronConsole.PrintLine("In override SetVolume 5 _currentVolume={0} volumeCommand.Command={1}", _currentVolume, volumeCommand.Command);

            StartRampingValue();

            if (volumeCommand != null)
            {
                SendCommand(volumeCommand);
            }

            Audio stateObj;
            stateObj = new Audio { MuteIsOn = MuteIsOn, VolumeIs = (uint)_currentVolume };
            UnscaledRampingVolumeIs = (uint)_currentVolume;
            UnscaledVolumeIs = (uint)_currentVolume;
            FireEvent(DisplayStateObjects.Audio, stateObj);

            //base.DeConstructVolume(Convert.ToString(_currentVolume));
        }
        ******/
       
        public override void SetVolume(uint volumeLev)
        {
            if (volumeLev > 100)
            {
                volumeLev = 100;
            }

            UnscaledVolumeIs = volumeLev;
            UnscaledRampingVolumeIs = UnscaledVolumeIs;

            string formattedVolume = Convert.ToString(volumeLev);

            Commands command = DisplayData.CrestronSerialDeviceApi.Api.StandardCommands[StandardCommandsEnum.Vol];
            var volumeParameter = ParameterHelper.GetFirstValidParameter(command);
            formattedVolume = ParameterHelper.FormatValue(formattedVolume, volumeParameter);
            string volumeParameterTag = "!$[" + volumeParameter.Id + "]";
            string modifiedCommand = ParameterHelper.ReplaceParameter(command.Command, volumeParameterTag, formattedVolume);

            CommandSet volumeCommand = BuildCommand(StandardCommandsEnum.Vol, CommonCommandGroupType.Volume,
                CommandPriority.Normal, "Volume " + Convert.ToString(volumeLev), modifiedCommand);

            if (volumeCommand != null)
            {
                SendCommand(volumeCommand);
            }

            ForceVolumeEvent(volumeLev);
            VolumePressed = true;
        }

        internal void ForceVolumeEvent(uint level)
        {
            byte[] b = BitConverter.GetBytes(level);
            string Hex = Encoding.GetString(b, 0, 1);
            DeConstructVolume(Hex);
        }
        


        public override void SetChannel(string channel)
        {
            for (int onChar = 0; onChar < channel.Length; onChar++)
            {
                //string channelChar = channel.Substring(onChar, 1);
                char val = Convert.ToChar(channel[onChar]);
                string numberString = string.Empty;
                string commandName = string.Empty;
                StandardCommandsEnum commandEnumValue = StandardCommandsEnum.Nop;
                switch (val)
                {
                    case '0':
                        {
                            numberString = "\u008C\u0000\u0067\u0003\u0001\u0009";
                            commandEnumValue = StandardCommandsEnum._0;
                            commandName = "0";
                            break;
                        }
                    case '1':
                        {
                            numberString = "\u008C\u0000\u0067\u0003\u0001\u0000";
                            commandEnumValue = StandardCommandsEnum._1;
                            commandName = "1";
                            break;
                        }
                    case '2':
                        {
                            numberString = "\u008C\u0000\u0067\u0003\u0001\u0001";
                            commandEnumValue = StandardCommandsEnum._2;
                            commandName = "2";
                            break;
                        }
                    case '3':
                        {
                            numberString = "\u008C\u0000\u0067\u0003\u0001\u0002";
                            commandEnumValue = StandardCommandsEnum._3;
                            commandName = "3";
                            break;
                        }
                    case '4':
                        {
                            numberString = "\u008C\u0000\u0067\u0003\u0001\u0003";
                            commandEnumValue = StandardCommandsEnum._4;
                            commandName = "4";
                            break;
                        }
                    case '5':
                        {
                            numberString = "\u008C\u0000\u0067\u0003\u0001\u0004";
                            commandEnumValue = StandardCommandsEnum._5;
                            commandName = "5";
                            break;
                        }
                    case '6':
                        {
                            numberString = "\u008C\u0000\u0067\u0003\u0001\u0005";
                            commandEnumValue = StandardCommandsEnum._6;
                            commandName = "6";
                            break;
                        }
                    case '7':
                        {
                            numberString = "\u008C\u0000\u0067\u0003\u0001\u0006";
                            commandEnumValue = StandardCommandsEnum._7;
                            commandName = "7";
                            break;
                        }
                    case '8':
                        {
                            numberString = "\u008C\u0000\u0067\u0003\u0001\u0007";
                            commandEnumValue = StandardCommandsEnum._8;
                            commandName = "8";
                            break;
                        }
                    case '9':
                        {
                            numberString = "\u008C\u0000\u0067\u0003\u0001\u0008";
                            commandEnumValue = StandardCommandsEnum._9;
                            commandName = "9";
                            break;
                        }
                    case '.':
                        {
                            numberString = "\u008C\u0000\u0067\u0003\u0097\u001D";
                            commandEnumValue = StandardCommandsEnum.Period;
                            commandName = ".";
                            break;
                        }
                    case ' ':
                        {
                            goto case '.';
                        }
                    case '-':
                        {
                            goto case '.';
                        }
                }
                
                CommandSet command = new CommandSet(commandName,
                                                    numberString,
                                                    CommonCommandGroupType.Keypad,
                                                    null,
                                                    true,
                                                    CommandPriority.Normal,
                                                    commandEnumValue);
                //if you don't sleep for a few seconds even though the commands are sent in order they will get mixed up
                CrestronEnvironment.Sleep(250);
                SendCommand(command);
            }
        }

        public override void ChannelUp()
        {
            StandardCommandsEnum commandEnumValue = StandardCommandsEnum.ChannelUp;
            string commandString = "\u008C\u0000\u0067\u0003\u0001\u0010";
            CommandSet command = new CommandSet("ChannelUp",
             commandString,
             CommonCommandGroupType.Channel,
             null,
             true,
             CommandPriority.Normal,
             commandEnumValue);
            SendCommand(command);
        }

        public override void ChannelDown()
        {
            StandardCommandsEnum commandEnumValue = StandardCommandsEnum.ChannelDown;
            string commandString = "\u008C\u0000\u0067\u0003\u0001\u0011";
            CommandSet command = new CommandSet("ChannelDown",
             commandString,
             CommonCommandGroupType.Channel,
             null,
             true,
             CommandPriority.Normal,
             commandEnumValue);
            SendCommand(command);
        }

        public override void ArrowKey(ArrowDirections direction)
        {
            string arrowKeyString = string.Empty;
            StandardCommandsEnum commandEnumValue = StandardCommandsEnum.Nop;

            switch (direction)
            {
                case ArrowDirections.Down:
                    arrowKeyString = "\u008C\u0000\u0067\u0003\u0001\u0075";
                    commandEnumValue = StandardCommandsEnum.DownArrow;
                    break;
                case ArrowDirections.Left:
                    arrowKeyString = "\u008C\u0000\u0067\u0003\u0001\u0034";
                    commandEnumValue = StandardCommandsEnum.LeftArrow;
                    break;
                case ArrowDirections.Right:
                    arrowKeyString = "\u008C\u0000\u0067\u0003\u0001\u0033";
                    commandEnumValue = StandardCommandsEnum.RightArrow;
                    break;
                case ArrowDirections.Up:
                    arrowKeyString = "\u008C\u0000\u0067\u0003\u0001\u0074";
                    commandEnumValue = StandardCommandsEnum.UpArrow;
                    break;
            }
            CommandSet command = new CommandSet("ArrowKey", arrowKeyString, CommonCommandGroupType.Arrow, null, true, CommandPriority.Normal, commandEnumValue);
            SendCommand(command);
        }
          
        public override void Home()
        {
            string cmd = "\x8C\x00\x67\x03\x01\x60";
            CommandSet command = new CommandSet("Home",
               cmd,
               CommonCommandGroupType.Other,
               null,
               true,
               CommandPriority.Normal, 
               StandardCommandsEnum.Home);

           SendCommand(command);
        }
        
        public override void Back()
        {
            string cmd = "\u008C\u0000\u0067\u0003\u0097\u0023";
            CommandSet command = new CommandSet("Return",
                   cmd,
                   CommonCommandGroupType.Other,
                   null,
                   true,
                   CommandPriority.Normal,
                   StandardCommandsEnum.Back);

            SendCommand(command);
        }

          public override void Select()
          {
              string cmd = "\u008C\u0000\u0067\u0003\u0001\u0065";
              CommandSet command = new CommandSet("Select",
                        cmd,
                        CommonCommandGroupType.Other,
                        null,
                        true,
                        CommandPriority.Normal,
                        StandardCommandsEnum.Select);

              SendCommand(command);
          }

          public override void Options()
          {
              string cmd = "\u008C\u0000\u0067\u0003\u0097\u0036";
              CommandSet command = new CommandSet("Options",
                        cmd,
                        CommonCommandGroupType.Other,
                        null,
                        true,
                        CommandPriority.Normal,
                        StandardCommandsEnum.Options);

              SendCommand(command);
          }
          public override void OnScreenDisplay()
          {
              string cmd = "\u008C\u0000\u0067\u0003\u0001\u003A";
              CommandSet command = new CommandSet("Display",
                        cmd,
                        CommonCommandGroupType.Other,
                        null,
                        true,
                        CommandPriority.Normal,
                        StandardCommandsEnum.OnScreenDisplay);
              SendCommand(command);
          }
        #endregion Commands 

    }

    public class AbsoluteValidator : ResponseValidation
    {
        public bool PowerOffIssued;
        private SonyXBRSeriesProtocol _protocol;

        public AbsoluteValidator(byte id, DataValidation dataValidation, SonyXBRSeriesProtocol protocol)
            : base(id, dataValidation)
        {
            Id = id;
            DataValidation = dataValidation;
            _protocol = protocol;
        }

        
        public override ValidatedRxData ValidateResponse(string response, CommonCommandGroupType commandGroup)
        {
            var validatedRxData = new ValidatedRxData(false, null);

            byte[] data = Encoding.GetBytes(response);

            if (data.Length == 1 && data[0] != 112)
            {
                //invalid response header. this is not the start of a response.
                validatedRxData.Ignore = true;
            }
            else if (data.Length > 1)
            {
                byte commandGroupHeader = data[1];

                if (data.Length > 2)
                {
                    byte commandDataLength = data[2];

                    if (response.Equals("\u0070\u0001\u0071") || response.Equals("\u0070\u0002\u0072") ||
                            response.Equals("\u0070\u0003\u0073") || response.Equals("\u0070\u0004\u0074"))
                    {
                        // Parse as NAK
                        return base.ValidateResponse(DataValidation.NakDefinition, CommonCommandGroupType.AckNak);
                    }
                    else
                    {
                        if (response.Equals("\u0070\u0000\u0070"))
                        {
                            validatedRxData.Ignore = true;
                        }
                        else
                        {
                            int dataLength = (int)commandDataLength + 3;

                            if (data.Length == dataLength)
                            {
                                //this should be a response. run it through the system
                                // There are  other replies to control requests that signal that the request was not executed

                                // There are some packets the display will send after a volume poll; ignore them
                                if (response.Contains("\u0070\u0004\u0001\u0075"))
                                {
                                    // Ignore this packet
                                    validatedRxData.Ignore = true;
                                }
                                else if ((commandGroup == CommonCommandGroupType.Volume) &&
                                            (_protocol.VolumePressed == true))
                                {
                                    // Ignore this packet
                                    validatedRxData.Ignore = true;
                                    _protocol.VolumePressed = false;
                                }
                                else
                                {
                                    string parsingValue = DataValidation.Feedback.Header;

                                    if (response.Contains(parsingValue) && response.Length >= parsingValue.Length + 2)// 2=[Data byte][Chksum byte]
                                    {
                                        // Remove checksum (last byte)
                                        response = response.Substring(0, response.Length - 1);

                                        //Antenna/TV input feedback also returns channel number, remove it
                                        if (validatedRxData.CommandGroup.Equals(CommonCommandGroupType.Input) && response.StartsWith("\u0003\u0001"))
                                        {
                                            response = response.Substring(0, response.Length - 1);
                                        }

                                        // Call base class
                                        return base.ValidateResponse(response, commandGroup);
                                    }
                                    else if (!response.Contains(parsingValue) && response.Length >= parsingValue.Length)
                                    {
                                        return new ValidatedRxData(true, DataValidation.NakDefinition) { CommandGroup = CommonCommandGroupType.AckNak };
                                    }
                                }
                            }
                            else if (data.Length > dataLength)
                            {
                                //command was not matched and too long. Give up and try again from the start.
                                validatedRxData.Ignore = true;
                            }
                        }
                    }
                }
            }

            return validatedRxData;
        }

        private CommonCommandGroupType ProcessCommandGroupType(string header)
        {
            
            CommonCommandGroupType type = CommonCommandGroupType.Unknown;

            if (header.Equals(DataValidation.Feedback.PowerFeedback.GroupHeader))
            {
                type = CommonCommandGroupType.Power;
            }
            else if (header.Equals(DataValidation.Feedback.MuteFeedback.GroupHeader))
            {
                type = CommonCommandGroupType.Mute;
            }
            else if (header.Equals(DataValidation.Feedback.InputFeedback.GroupHeader))
            {
                type = CommonCommandGroupType.Input;
            }
            else if (header.Equals(string.Format("{0}{1}",DataValidation.Feedback.Header,"\u0003\u0001")))
            {
                type = CommonCommandGroupType.Volume;
            }

            return type;
        }
    }
}
