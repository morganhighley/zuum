using System;
using System.Collections.Generic;
using System.Linq;

using Crestron.RAD.Common.ExtensionMethods;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Http;
using System.Reflection;

namespace Crestron.RAD.Drivers.Displays
{
    using Crestron.RAD.Common;
    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Helpers;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.Display;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Crestron.SimplSharp.CrestronSockets;
    using System.Text.RegularExpressions;
    using Crestron.RAD.Ext.Util.Scaling;
    using DriverExtensionLibrary.Helpers;


    public class CmdRootObject
    {
        public string method { get; set; }
        public int id { get; set; }
        public List<Params> @params { get; set; }
        public string version { get; set; }
    }

    public class Params
    {
        public string source { get; set; }
        public string volume { get; set; }
        public string target { get; set; }
        public bool? status { get; set; }
        public string uri { get; set; }
        public string channel { get; set; }
        public string mode { get; set; }
    }

    public class ChannelSequenceConfig
    {
        public uint MinimumNumberOfDigits { get; set; }
        public bool TriggerEnterAfterCommands { get; set; }
        public uint DelayBetweenCommands { get; set; }
        public uint DelayBetweenSequences { get; set; }
        public uint IRCommandDuration { get; set; }
    }

    public class SonyXBRSeriesTcpProtocol : ADisplayProtocol
    {
        // These were added in after-the-fact to improve the responsiveness
        // of mute feedback. This is why it feels tacked-on -- it is. There's
        // not a lingering reason why this couldn't be done for other commands.
        public enum CommandId
        {
            Illegal = 0,
            Default = 1,
            MuteOff = 10,
            MuteOn = 11,
        }

        // Class to wrap IAdjustableLevel so the class doesn't pick up a
        // confusing public method and interface
        private class AdjustableLevel : IAdjustableLevel
        {
            private readonly Action<double> _adjustSteps;

            public AdjustableLevel(Action<double> adjustSteps)
            {
                _adjustSteps = adjustSteps;
            }

            public void AdjustSteps(double direction)
            {
                _adjustSteps(direction);
            }
        }

        private IPAddress _devIpAddress;
        private readonly ISerialTransport _transport;
        private static string _apiVersion = "1.0";
        private const string _placeHolderResponse = "place holder";

        internal string PSK { get; set; } //Pre-shared Key from Device, Needed for control

        private int _currentVolume;
        
        // Timer for volume ramping
        private ScheduledEventTimer _rampingTimer;
        private readonly long[] _rampingSchedule = new long[] { 0, 500, 250, 100 };

        // Controller that connects the ramping timer to the volume level
        private LevelRamper _volumeRamper;

        private ChannelSequenceConfig _channelSeqConfig;
        private CTimer _setChanTimer;
        private bool _canSetChannel = true;

        private bool _powerSavingOff = false;

        private Regex _regex;
        private RegexOptions _options = RegexOptions.Multiline;
        string _pattern = @"(?<Response>{""result"":[\[](?<ResponseContant>(?<PowerSavingResponse>{""mode"":""(?<PowerSavingValue>on|off)(?<PowerSavingResponseFooter>"".+))|(?<VolumeResponse>[\[]{""target"":""speaker"",""volume"":(?<VolumeLevelValue>[0-9]{1,3}),""mute"":(?<VolumeMuteValue>true|false)(?<VolumeResponseFooter>,.+))|(?<PowerResponse>{""status"":""(?<PowerValue>active|standby).+)|(?<InputResponse>{""uri"":""extInput:(?<InputInfo>.+),""title"":""(?<InputName>.+)""}))|0[\]](?<ResponseIDBody>,""id"":(?<ResponseID>[0-9]{1,3}))}|(?<MediaServiceResponse>{""error"":[\[]7,""Illegal State""[\]],""id"":\d+}))";

        public SonyXBRSeriesTcpProtocol(ISerialTransport transportDriver, byte id, IPAddress ipAddress)
            : base(transportDriver, id)
        {
            _devIpAddress = ipAddress;

            ResponseValidation = new SonyXBRSeriesResponseValidation(Id, ValidatedData);

            ValidatedData.PowerOnPollingSequence = new[] { 
                StandardCommandsEnum.PowerPoll,
                StandardCommandsEnum.InputPoll,  
                StandardCommandsEnum.VolumePoll  //Only one command is used for both volume and mute polls 
            };
            PollingInterval = 3000;
            _transport = transportDriver;

            _channelSeqConfig = new ChannelSequenceConfig
            {
                DelayBetweenCommands = 250,                   //Default 25 ms
                DelayBetweenSequences = 2000,                  //Default 2 seconds
                IRCommandDuration = 100,                         //Default 10 ms
                MinimumNumberOfDigits = 1,
                TriggerEnterAfterCommands = true
            };
            _setChanTimer = new CTimer(ClearSetChannel, _channelSeqConfig.DelayBetweenSequences);
            _regex = new Regex(_pattern, _options);

            _rampingTimer = new ScheduledEventTimer(_rampingSchedule);
            _volumeRamper = new LevelRamper(new AdjustableLevel(this.AdjustVolume), _rampingTimer) { StepsPerTick = 1 };
        }

        public override void Initialize(object driverData)
        {
            base.Initialize(driverData);
            ValidateResponse = DriverValidateResponse;
        }

        public override void Dispose()
        {
            _volumeRamper.Dispose();
            _rampingTimer.Dispose();
            base.Dispose();
        }

        protected override void Poll()
        {
            if (!_powerSavingOff)
            {
                _powerSavingOff = true;
                CommandSet command = new CommandSet("SetPowerSavingMode", _setPowerSavingMode, CommonCommandGroupType.EnergyStar, null, false, CommandPriority.Normal, StandardCommandsEnum.EnergyStar);
                SendCommand(command);
            }
            else
            {
                CommandSet pollCommand = new CommandSet("GetPowerSavingMode", "getPowerSavingMode", CommonCommandGroupType.EnergyStar, null, false, CommandPriority.Lowest, StandardCommandsEnum.EnergyStar);
                SendCommand(pollCommand);   
            }
            base.Poll();
        }

        #region User Attributes

        public override void SetUserAttribute(string attributeId, string attributeValue)
        {
            if (string.IsNullOrEmpty(attributeValue))
            {
                if (EnableLogging)
                {
                    Log("User attribute value was null or empty");
                }
            }
            else
            {
                switch (attributeId)
                {
                    case "OnScreenId":
                        PSK = attributeValue;
                        if (EnableLogging)
                        {
                            Log("Pre-Shared Key has been set");
                        }
                        break;
                }
            }
        }
        #endregion User Attributes

        //The url changes depending on the command being sent 
        public string getUrlServiceName(CommandSet commandSet)
        {
            //the service name for TVTuner media service is different than all the other media services
            if (commandSet.CommandName.Equals("TV"))
            {
                return "avContent";
            }
            switch (commandSet.CommandGroup)
            {
                case CommonCommandGroupType.Volume:
                case CommonCommandGroupType.Mute:
                    return "audio";

                case CommonCommandGroupType.Input:
                    return "avContent";

                case CommonCommandGroupType.Power:
                case CommonCommandGroupType.EnergyStar:
                    return "system";

                case CommonCommandGroupType.MediaService:
                    return "appControl";

                default:
                    return "IRCC";   //all other commands 
            }
        }



        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            bool result = false;

            //formats body and parameters to json depending on the command
            //adding a @@@@ as a delimiter to extract the service name in the transport
            switch (commandSet.CommandGroup)       
            {
                case CommonCommandGroupType.Other:
                case CommonCommandGroupType.Arrow:
                case CommonCommandGroupType.Keypad:
                    commandSet.Command = String.Format("{0}@@@@{1}", getUrlServiceName(commandSet), constructOtherObject(commandSet));
                    break;

                case CommonCommandGroupType.Power:
                    commandSet.Command = String.Format("{0}@@@@{1}", getUrlServiceName(commandSet), constructPowerObject(commandSet));
                    break;

                case CommonCommandGroupType.Mute:
                    commandSet.Command = String.Format("{0}@@@@{1}", getUrlServiceName(commandSet), constructMuteObject(commandSet));
                    break;

                case CommonCommandGroupType.Input:
                    commandSet.Command = String.Format("{0}@@@@{1}", getUrlServiceName(commandSet), constructInputObject(commandSet)); ;
                    break;

                case CommonCommandGroupType.Volume:
                    commandSet.Command = String.Format("{0}@@@@{1}", getUrlServiceName(commandSet), constructVolObject(commandSet));
                    break;

                case CommonCommandGroupType.MediaService:
                    commandSet.Command = String.Format("{0}@@@@{1}", getUrlServiceName(commandSet), constructMediaObject(commandSet));
                    break;
                case CommonCommandGroupType.EnergyStar:
                    commandSet.Command = String.Format("{0}@@@@{1}", getUrlServiceName(commandSet), constructPowerSavingObject(commandSet));
                    break;
            }

            result = base.PrepareStringThenSend(commandSet);

            return result;
        }

        #region responses

        public void ExternalDeConstructChannel(string response)
        {
            try
            {
                int stopAt = response.IndexOf("&");
                if (stopAt >= 0)
                {
                    int startAt = 36;
                    int numberChars = stopAt - startAt;
                    string channel = response.Substring(startAt, numberChars);
                    DeConstructChannel(channel);
                }
                else if (EnableLogging)
                {
                    Log("Couldn't find Channel Number");
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Log("Error Deconstructing Channel: " + e.Message);
                }
            }
            
        }

        #endregion

        #region commands
        public override void Select()
        {
            CommandSet command = new CommandSet("Select", "AAAAAgAAAJcAAABKAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Select);
            SendCommand(command);
        }

        public override void ArrowKey(ArrowDirections direction)
        {
            string arrowKeyString = string.Empty;
            StandardCommandsEnum commandEnumValue = StandardCommandsEnum.Nop;

            switch (direction)
            {
                case ArrowDirections.Down:
                    arrowKeyString = "AAAAAQAAAAEAAAB1Aw==";
                    commandEnumValue = StandardCommandsEnum.DownArrow;
                    break;
                case ArrowDirections.Left:
                    arrowKeyString = "AAAAAQAAAAEAAAA0Aw==";
                    commandEnumValue = StandardCommandsEnum.LeftArrow;
                    break;
                case ArrowDirections.Right:
                    arrowKeyString = "AAAAAQAAAAEAAAAzAw==";
                    commandEnumValue = StandardCommandsEnum.RightArrow;
                    break;
                case ArrowDirections.Up:
                    arrowKeyString = "AAAAAQAAAAEAAAB0Aw==";
                    commandEnumValue = StandardCommandsEnum.UpArrow;
                    break;
            }

            CommandSet command = new CommandSet("ArrowKey",
                arrowKeyString,
                CommonCommandGroupType.Arrow,
                null,
                false,
                CommandPriority.Normal,
                commandEnumValue);

            SendCommand(command);
        }

        public override void Menu()
        {
            CommandSet command = new CommandSet("Menu", "AAAAAgAAABoAAABgAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Menu);
            SendCommand(command);
        }

        public override void Home()
        {
            CommandSet command = new CommandSet("Home", "AAAAAQAAAAEAAABgAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Home);
            SendCommand(command);
        }
        public override void Play()
        {
            CommandSet command = new CommandSet("Play", "AAAAAgAAAJcAAAAaAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Play);
            SendCommand(command);
        }

        public override void Stop()
        {
            CommandSet command = new CommandSet("Stop", "AAAAAgAAAJcAAAAYAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Stop);
            SendCommand(command);
        }

        public override void Pause()
        {
            CommandSet command = new CommandSet("Pause", "AAAAAgAAAJcAAAAZAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Pause);
            SendCommand(command);
        }

        public override void Back()
        {
            CommandSet command = new CommandSet("Return", "AAAAAgAAAJcAAAAjAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Return);
            SendCommand(command);
        }

        public override void Exit()
        {   //same command as return 
            CommandSet command = new CommandSet("Exit", "AAAAAgAAAJcAAAAjAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Exit);
            SendCommand(command);
        }

        public override void ForwardScan()
        {
            CommandSet command = new CommandSet("ForwardScan", "AAAAAgAAAJcAAAAcAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.ForwardScan);
            SendCommand(command);
        }

        public override void ReverseScan()
        {
            CommandSet command = new CommandSet("ReverseScan", "AAAAAgAAAJcAAAAbAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.ForwardScan);
            SendCommand(command);
        }

        // Increment or decrement based on direction
        private void AdjustVolume(double direction)
        {
            int vol = (int)Math.Round(_currentVolume + direction);
            uint uvol = (uint)(vol > 0 ? vol : 0);

            if (uvol > MaxVolume)
            {
                uvol = MaxVolume;
            }

            if (uvol != _currentVolume)
            {
                SetVolume(uvol);
            }
        }

        public override void PressVolumeUp()
        {
            _volumeRamper.Start(true);
            // Disable polling while ramping volume to avoid stutters
            PollingEnabled = false;
        }

        public override void PressVolumeDown()
        {
            _volumeRamper.Start(false);
            // Disable polling while ramping volume to avoid stutters
            PollingEnabled = false;
        }

        public override void ReleaseVolume()
        {
            _volumeRamper.Stop();
            // Re-enable polling when ramping finishes
            PollingEnabled = true;
        }

        public override void SetVolume(uint volume)
        {
            _currentVolume = (int)volume;

            string vol = volume.ToString();

            CommandSet volumeCommand = new CommandSet("SetVolume", vol, CommonCommandGroupType.Volume, null, false, CommandPriority.Normal, StandardCommandsEnum.Vol);

            if (volumeCommand != null)
            {
                SendCommand(volumeCommand);
            }

            Audio stateObj;
            MuteIsOn = false;
            stateObj = new Audio { MuteIsOn = MuteIsOn, VolumeIs = (uint)_currentVolume };
            UnscaledRampingVolumeIs = (uint)_currentVolume;
            UnscaledVolumeIs = (uint)_currentVolume;
            FireEvent(DisplayStateObjects.Audio, stateObj);
            DeConstructVolume(Convert.ToString(_currentVolume));
        }

        public void VolumePollWithNormalPriority()
        {
            CommandSet command = BuildCommand(StandardCommandsEnum.VolumePoll, CommonCommandGroupType.Volume, CommandPriority.Normal, "Volume Poll");

            if (command != null)
            {
                SendCommand(command);
            }
        }

        protected override void ChooseDeconstructMethod(ValidatedRxData validatedData)
        {
            if (!validatedData.Data.Contains(_placeHolderResponse))
            {
                base.ChooseDeconstructMethod(validatedData);
            }
        }

        protected override void DeConstructVolume(string response)
        {
            int responseValue = Convert.ToInt32(response);

            if (!MuteIsOn)
            {
                _currentVolume = responseValue;
                base.DeConstructVolume(response);
            }
        }

        public override void Options()
        {
            CommandSet command = new CommandSet("Options", "AAAAAgAAAJcAAAA2Aw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Options);
            SendCommand(command);
        }

        public override void ColorButton(ColorButtons color)
        {
            CommandSet command = null;
            switch (color)
            {
                case ColorButtons.Blue:
                    command = new CommandSet("Blue", "AAAAAgAAAJcAAAAkAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Blue);
                    break;
                case ColorButtons.Green:
                    command = new CommandSet("Green", "AAAAAgAAAJcAAAAmAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Green);
                    break;
                case ColorButtons.Red:
                    command = new CommandSet("Red", "AAAAAgAAAJcAAAAlAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Red);
                    break;
                case ColorButtons.Yellow:
                    command = new CommandSet("Yellow", "AAAAAgAAAJcAAAAnAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Yellow);
                    break;
            }
            if (command != null)
            {
                SendCommand(command);
            }
        }

        public override void ChannelUp()
        {
            CommandSet command = new CommandSet("Channel Up", "AAAAAQAAAAEAAAAQAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.ChannelUp);
            SendCommand(command);
        }

        public override void ChannelDown()
        {
            CommandSet command = new CommandSet("Channel Down", "AAAAAQAAAAEAAAARAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.ChannelDown);
            SendCommand(command);
        }

        #region Set Channel

        private void ClearSetChannel(object obj)
        {
            _setChanTimer.Stop();
            _canSetChannel = true;
        }

        public bool SupportsDash { get { return false; } }
        public bool SupportsPeriod { get { return true; } }

        public override void SetChannel(string channel)
        {
            if (_canSetChannel & channel.Length > 0)
            {
                _canSetChannel = false;
                
                for (int onChar = 0; onChar < channel.Length; onChar++)
                {
                    char val = Convert.ToChar(channel[onChar]);
                    string numberString = string.Empty;
                    string commandName = string.Empty;
                    StandardCommandsEnum commandEnumValue = StandardCommandsEnum.Nop;
                    switch (val)
                    {
                        case '0':
                            {
                                numberString = "AAAAAQAAAAEAAAAJAw==";
                                commandEnumValue = StandardCommandsEnum._0;
                                commandName = "0";
                                break;
                            }
                        case '1':
                            {
                                numberString = "AAAAAQAAAAEAAAAAAw==";
                                commandEnumValue = StandardCommandsEnum._1;
                                commandName = "1";
                                break;
                            }
                        case '2':
                            {
                                numberString = "AAAAAQAAAAEAAAABAw==";
                                commandEnumValue = StandardCommandsEnum._2;
                                commandName = "2";
                                break;
                            }
                        case '3':
                            {
                                numberString = "AAAAAQAAAAEAAAACAw==";
                                commandEnumValue = StandardCommandsEnum._3;
                                commandName = "3";
                                break;
                            }
                        case '4':
                            {
                                numberString = "AAAAAQAAAAEAAAADAw==";
                                commandEnumValue = StandardCommandsEnum._4;
                                commandName = "4";
                                break;
                            }
                        case '5':
                            {
                                numberString = "AAAAAQAAAAEAAAAEAw==";
                                commandEnumValue = StandardCommandsEnum._5;
                                commandName = "5";
                                break;
                            }
                        case '6':
                            {
                                numberString = "AAAAAQAAAAEAAAAFAw==";
                                commandEnumValue = StandardCommandsEnum._6;
                                commandName = "6";
                                break;
                            }
                        case '7':
                            {
                                numberString = "AAAAAQAAAAEAAAAGAw==";
                                commandEnumValue = StandardCommandsEnum._7;
                                commandName = "7";
                                break;
                            }
                        case '8':
                            {
                                numberString = "AAAAAQAAAAEAAAAHAw==";
                                commandEnumValue = StandardCommandsEnum._8;
                                commandName = "8";
                                break;
                            }
                        case '9':
                            {
                                numberString = "AAAAAQAAAAEAAAAIAw==";
                                commandEnumValue = StandardCommandsEnum._9;
                                commandName = "9";
                                break;
                            }
                        case '.':
                            {
                                numberString = "AAAAAgAAAJcAAAAdAw==";
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

                    SendCommand(command);   
                }

                if (_channelSeqConfig.TriggerEnterAfterCommands)
                {
                    Enter();
                }

                _canSetChannel = true;
            }
        }
        #endregion

        public override void Period()
        {
            CommandSet command = new CommandSet("Period", "AAAAAgAAAJcAAAAdAw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Period);
            SendCommand(command);
        }

        public override void ForwardSkip()
        {
            CommandSet command = new CommandSet("ForwardSkip", "AAAAAgAAAJcAAAA9Aw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.ForwardSkip);
            SendCommand(command);
        }

        public override void ReverseSkip()
        {
            CommandSet command = new CommandSet("ReverseSkip", "AAAAAgAAAJcAAAA8Aw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.ReverseSkip);
            SendCommand(command);
        }

        public override void Info()
        {
            //called 'Display' in the API
            CommandSet command = new CommandSet("Info", "AAAAAQAAAAEAAAA6Aw==", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Info);
            SendCommand(command);
        }

        //Since this driver does not provice active media service feedback I am forcing the deconstruct to oen of two choices
        //tvtuner for Live TV and UnknownMediaService for all other apps.    This has corrected the issue with the Home button
        //by removing it from the live tv app as directed and restoring it when another aspp is selected.  
        private string _currentMediaService = string.Empty;
        private static string _tvMediaService = "tvtuner";
        private static string _smartTvMediaService = "unknownMediaService";

        public override void SelectMediaService(string mediaServiceId)
        {
            if (!mediaServiceId.Equals(_tvMediaService))
            {
                _currentMediaService = _smartTvMediaService;
            }
            else
            {
                _currentMediaService = _tvMediaService;
            }

            FireEvent(DisplayStateObjects.ActiveMediaService, _currentMediaService);
            base.SelectMediaService(mediaServiceId);
        }

        public override void DeConstructActiveMediaServiceFeedback(string response)
        {
            if (!response.Equals(_currentMediaService))
            {
                base.DeConstructActiveMediaServiceFeedback(_currentMediaService);
            }
            else
            {
                base.DeConstructActiveMediaServiceFeedback(response);
            }
        }

        #endregion commands

        #region commandFormatting

        private string constructOtherObject(CommandSet commandSet)
        {
            string xml = string.Empty;
            xml = string.Format(@"
                <s:Envelope
                xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""
                s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                <s:Body>
                <u:X_SendIRCC xmlns:u=""urn:schemas-sony-com:service:IRCC:1"">
                <IRCCCode>{0}</IRCCCode>
                </u:X_SendIRCC>
                </s:Body>
                </s:Envelope>", commandSet.Command);
            return xml;
        }

        private static string _powerPollMethod = "getPowerStatus";
        private string constructPowerObject(CommandSet commandSet)
        {
            var cmd = new CmdRootObject();
            switch (commandSet.StandardCommand)
            {
                case StandardCommandsEnum.PowerOn:
                    cmd.id = (int)CommandId.Default;
                    cmd.method = commandSet.Command;
                    cmd.version = _apiVersion;
                    cmd.@params = new List<Params> {new Params
                        {status = true}};
                    break;

                case StandardCommandsEnum.PowerOff:
                    cmd.id = (int)CommandId.Default;
                    cmd.method = commandSet.Command;
                    cmd.version = _apiVersion;
                    cmd.@params = new List<Params> {new Params
                        {status = false}};
                    break;

                case StandardCommandsEnum.PowerPoll:
                    cmd.id = (int)CommandId.Default;
                    cmd.method = _powerPollMethod;
                    cmd.version = _apiVersion;
                    cmd.@params = new List<Params> {new Params
                        {status = null}};
                    break;
            }

            return JsonConvert.SerializeObject(cmd, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private string constructMuteObject(CommandSet commandSet)
        {
            var cmd = new CmdRootObject();
            switch (commandSet.StandardCommand)
            {
                case StandardCommandsEnum.MuteOn:
                    cmd.id = (int)CommandId.MuteOn;
                    cmd.method = commandSet.Command;
                    cmd.version = _apiVersion;
                    cmd.@params = new List<Params> {new Params
                        {status = true}};
                    break;

                case StandardCommandsEnum.MuteOff:
                    cmd.id = (int)CommandId.MuteOff;
                    cmd.method = commandSet.Command;
                    cmd.version = _apiVersion;
                    cmd.@params = new List<Params> {new Params
                        {status = false}};
                    break;

                case StandardCommandsEnum.MutePoll:
                    cmd.id = (int)CommandId.Default;
                    cmd.method = commandSet.Command;
                    cmd.version = _apiVersion;
                    cmd.@params = new List<Params> {new Params
                        {status = null}};
                    break;

            }
            return JsonConvert.SerializeObject(cmd, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private static string _cmdInputMethod = "setPlayContent";
        private string constructInputObject(CommandSet commandSet)
        {
            CmdRootObject cmd = new CmdRootObject();
            if (commandSet.StandardCommand.Equals(StandardCommandsEnum.InputPoll))
            {

                cmd.id = (int)CommandId.Default;
                cmd.method = commandSet.Command;
                cmd.version = _apiVersion;
                cmd.@params = new List<Params> {new Params
                            {status = null}};
            }
            else if (commandSet.Command.StartsWith("extInput"))
            {
                cmd.id = (int)CommandId.Default;
                cmd.method = _cmdInputMethod;   //method for changing input will be the same
                cmd.version = _apiVersion;
                cmd.@params = new List<Params> {new Params
                            {uri = commandSet.Command,
                             status = null}};    //uri changes input    
            }


            return JsonConvert.SerializeObject(cmd, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private static string _setVolMethod = "setAudioVolume";
        private string constructVolObject(CommandSet commandSet)
        {
            CmdRootObject cmd = new CmdRootObject();
            switch (commandSet.StandardCommand)
            {
                case StandardCommandsEnum.Vol:
                    cmd.id = (int)CommandId.Default;
                    cmd.method = _setVolMethod;
                    cmd.version = _apiVersion;
                    cmd.@params = new List<Params> {new Params
                        {volume = commandSet.Command,
                         target = "",
                         status = null}};
                    break;

                case StandardCommandsEnum.VolPlus:
                case StandardCommandsEnum.VolMinus:
                    cmd.id = (int)CommandId.Default;
                    cmd.method = _setVolMethod;
                    cmd.version = _apiVersion;
                    cmd.@params = new List<Params> {new Params
                        {volume = commandSet.Command,
                         target = "",
                         status = null}};
                    break;

                case StandardCommandsEnum.VolumePoll:
                    cmd.id = (int)CommandId.Default;
                    cmd.method = commandSet.Command;
                    cmd.version = _apiVersion;
                    cmd.@params = new List<Params> {new Params
                        {status = null}};
                    break;
            }
            return JsonConvert.SerializeObject(cmd, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private static string _setActiveAppMethod = "setActiveApp";
        private string constructMediaObject(CommandSet commandSet)
        {
            CmdRootObject cmd = new CmdRootObject();

            //TVTuner Media Service has a different method
            if (commandSet.CommandName.Equals("TV"))
            {
                cmd.id = (int)CommandId.Default;
                cmd.method = _cmdInputMethod;  
                cmd.version = _apiVersion;
                cmd.@params = new List<Params> {new Params
                            {uri = commandSet.Command,
                             status = null}};   
            }
            else
            {
                //all other media services
                cmd.id = (int)CommandId.Default;
                cmd.method = _setActiveAppMethod;
                cmd.version = _apiVersion;
                cmd.@params = new List<Params> {new Params
                            {uri = commandSet.Command, //uri changes media service
                             status = null}};
            }
            return JsonConvert.SerializeObject(cmd, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private static string _setPowerSavingMode = "setPowerSavingMode";

        private string constructPowerSavingObject(CommandSet commandSet)
        {
            CmdRootObject cmd = new CmdRootObject();
            if (commandSet.CommandGroup == CommonCommandGroupType.EnergyStar)
            {
                cmd.id = (int)CommandId.Default;
                cmd.method = commandSet.Command;
                cmd.version = _apiVersion;
                cmd.@params = new List<Params> {new Params
                            {mode = "off"}};   
            }

            string serialisedPowerSave = JsonConvert.SerializeObject(cmd, Formatting.None,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

            return serialisedPowerSave;
        }

        #endregion commandFormatting

        /*Looks for the inputs in the string based on wether the connection is true or not
         * The response sends a Json with the connection status of every Input
         */
        private string ProcessInputFeedback(string response)
        {
            string connectedInput = string.Empty;
            try
            {
                connectedInput = response;

                //sometimes inputs returns with '\' such as 'Video 2\/Component' and 'HDMI 3\/ARC'
                if (connectedInput.Contains("\\"))
                {
                    connectedInput = connectedInput.Replace("\\", string.Empty);
                }

                //some models will return HDMI 3 as HDMI 3/ARC and others just as HDMI 3, Removing "/ARC"l
                if (connectedInput.Contains("/"))
                {
                    connectedInput = connectedInput.Substring(0, connectedInput.LastIndexOf("/"));
                }
            }
            catch (Exception e)
            {

                if (EnableLogging)
                {
                    Log(string.Format("Error found while processing input feedback - {0}", e.Message));
                }
            }

            return connectedInput;
        }

        public override void PowerOn()
        {
            base.PowerOn();
        }

        public override void SetVideoInput(VideoConnections input)
        {
            base.SetVideoInput(input);
        }

        public override void SetInput(VideoConnections videoConnection)
        {
            base.SetInput(videoConnection);
        }

        public ValidatedRxData DriverValidateResponse(string response, CommonCommandGroupType commandGroup)
        {
            ValidatedRxData validatedData = new ValidatedRxData(false, string.Empty);

            //Nak reponses
            if (response.Contains("Bad Request") || response.Contains("Forbidden") ||
                response.Contains("Unauthorized") || response.Contains("Not Found") ||
                response.Contains("Cannot accept the IRCC Code") ||
                response.Contains("500 Internal Service Error"))
            {
                if (response.Contains("Forbidden") && EnableLogging)  //returned when psk is wrong
                {
                    Log("Wrong Pre-Shared Key entered");
                }

                validatedData.Data = ValidatedData.NakDefinition;
                validatedData.CommandGroup = CommonCommandGroupType.AckNak;
                validatedData.Ignore = true;

                if (EnableLogging)
                {
                    Log("NAK Received");
                }
            }
            else if (_regex is Regex)
            {
                GroupCollection groups = _regex.Match(response).Groups;
                if (groups.Count > 0)
                {
                    Group groupPowerSavingValue = groups["PowerSavingValue"];
                    Group groupVolumeLevelValue = groups["VolumeLevelValue"];
                    Group groupVolumeMuteValue = groups["VolumeMuteValue"];
                    Group groupPowerValue = groups["PowerValue"];
                    Group groupInputName = groups["InputName"];
                    Group groupMediaServiceResponse = groups["MediaServiceResponse"];
                    Group groupResponseId = groups["ResponseID"];

                    CommandId cmdId = CommandId.Default;
                    try
                    {
                        if (groupResponseId.Value.Length > 0)
                        {
                            cmdId = (CommandId)int.Parse(groupResponseId.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (EnableLogging)
                        {
                            Log(string.Format("Error parsing command response ID: {0}", ex.ToString()));
                        }
                    }

                    //If a response contains any of these things the Response Validation will call the Deonstruct directly.
                    //This allows any combination of these responses to be handled in one pass to enhance the performance of the driver

                    if (groupVolumeMuteValue.Value.Length > 0)
                    {
                        DeConstructMute(groupVolumeMuteValue.Value);
                    }
                    else if (cmdId == CommandId.MuteOn)
                    {
                        DeConstructMute("true");
                    }
                    else if (cmdId == CommandId.MuteOff)
                    {
                        DeConstructMute("false");
                    }

                    if (groupVolumeLevelValue.Value.Length > 0)
                    {
                        DeConstructVolume(groupVolumeLevelValue.Value);
                    }

                    if (groupPowerValue.Value.Length > 0)
                    {
                        DeConstructPower(groupPowerValue.Value);
                    }

                    if (groupInputName.Value.Length > 0)
                    {
                        validatedData.Data = ProcessInputFeedback(groupInputName.Value);

                        //screen mirrorring app feedback is returned like an input. Setting it as a media service
                        if (validatedData.Data.EndsWith("Screen mirroring"))
                        {
                            validatedData.Data = "MediaService";
                        }
                        DeConstructInput(validatedData.Data);
                    }

                    if (groupMediaServiceResponse.Value.Length > 0)
                    {
                        DeConstructInput("MediaService");
                    }

                    //this response doesn't require a DeConstruct
                    if (groupPowerSavingValue.Value.Length > 0)
                    {
                        _powerSavingOff = groupPowerSavingValue.Value.Equals("off");
                    }
                    
                    //I am purposly sending this back as the validated data because all deConstructs have been called in this method as needed
                    //At this point I am just supressing a message timeout error message
                    validatedData.Data = _placeHolderResponse;
                    validatedData.CommandGroup = CommonCommandGroupType.Unknown;
                    validatedData.Ready = true;
                }
                else
                {
                    validatedData.Ignore = true;
                }
            }
            else
            {
                validatedData.Ignore = true;
            }

            return validatedData;
        }
    }
}
