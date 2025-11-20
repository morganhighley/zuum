// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="TivoProtocol.cs">
//   
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Text;
using Crestron.RAD.Common.StandardCommands;
using Crestron.SimplSharp;
using Crestron.RAD.Common;
using Crestron.RAD.Common.ExtensionMethods;
using Crestron.RAD.Common.Helpers;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.DeviceTypes.CableBox;


namespace Crestron.RAD.Drivers.CableBoxes
{
   
    public class TivoProtocol : ACableBoxProtocol
    {
        private ChannelSequenceConfig _channelSeqConfig;
        private CTimer _setChanTimer;
        private bool _canSetChannel = true;
        private ushort _useDefaultPowerOnCommand = 1;
        private ushort _useDefaultPowerOffCommand = 1;

        public TivoProtocol(ISerialTransport transportDriver, byte id) : base(transportDriver, id)
        {
            ResponseValidation = new DelimiterValidator(ValidatedData);
            
            // Treat this device as always being powered on
            PowerIsOn = true;
            FireEvent(CableBoxStateObjects.Power, new Power { PowerIsOn = PowerIsOn });

            _channelSeqConfig = new ChannelSequenceConfig
            {
                DelayBetweenCommands = 250,                   //Default 25 ms
                DelayBetweenSequences = 2000,                  //Default 2 seconds
                IRCommandDuration = 100,                         //Default 10 ms
                MinimumNumberOfDigits = 1,
                TriggerEnterAfterCommands = true
            };
            _setChanTimer = new CTimer(ClearSetChannel, _channelSeqConfig.DelayBetweenSequences);
        }

        public override void Options()
        {
            CommandSet command = new CommandSet("Options", "IRCODE LIVETV\\u000D", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Options);
            SendCommand(command);
        }

        public override void PowerOn()
        {           
            if (_useDefaultPowerOnCommand == 1)
            {
                base.PowerOn();
            }
            else
            {
                //configuration is that no command to be sent as PowerOn
                Log("In override PowerOn sending Nothing for PowerOn command");
            }
        }


        public override void PowerOff()
        {
            if (_useDefaultPowerOffCommand == 1)
            {
                base.PowerOff();
            }
            else
            {
                //configuration is that no command to be sent as PowerOff
                Log("In override PowerOff sending Nothing for PowerOff command");
            }
        }


        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            string localcommand2 = commandSet.Command.Substring(0,9);

            if (commandSet.CommandGroup == CommonCommandGroupType.Channel)
            {
                if (commandSet.Command.Contains("-") || commandSet.Command.Contains("."))
                {
                    string newCommand = commandSet.Command.Replace("-", " ").Replace("."," ");
                    commandSet.Command = newCommand;
                }
            }
            return base.PrepareStringThenSend(commandSet);
        }

        private void ClearSetChannel(object obj)
        {
            _setChanTimer.Stop();
            _canSetChannel = true;
        }

        public bool SupportsDash { get { return true; }}
        public bool SupportsPeriod { get { return false; }}

        /*public override void SetChannel(string channel)
        {
            base.SetChannel(channel);
        }*/


        public override void SetUserAttribute(string attributeId, ushort attributeValue)
        {
            switch (attributeId)
            {
                case "customPowerOnCommand":
                    if ((attributeValue == 0) || (attributeValue == 1))
                    {
                        _useDefaultPowerOnCommand = attributeValue;
                    }
                    else
                    {
                        _useDefaultPowerOnCommand = 1;
                    }

                    break;

                case "customPowerOffCommand":
                    if ((attributeValue == 0) || (attributeValue == 1))
                    {
                        _useDefaultPowerOffCommand = attributeValue;
                    }
                    else
                    {
                        _useDefaultPowerOffCommand = 1;
                    }

                    break;
            }
        }

        public override  void SetChannel(string channel)
        {
            if (_canSetChannel)
            {
                _canSetChannel = false;
                _setChanTimer.Reset(_channelSeqConfig.DelayBetweenSequences);

                if (channel.Length > 0)
                {
                    for (int i = 0; i < channel.Length; i++)
                    {
                        _setChanTimer.Reset(_channelSeqConfig.DelayBetweenSequences);
                        char val = Convert.ToChar(channel[i]);

                        StandardCommandsEnum standardCommand = StandardCommandsEnum.Nop;

                        switch (val)
                        {
                            case '0':
                                standardCommand = StandardCommandsEnum._0;
                                break;
                            case '1':
                                standardCommand = StandardCommandsEnum._1;
                                break;
                            case '2':
                                standardCommand = StandardCommandsEnum._2;
                                break;
                            case '3':
                                standardCommand = StandardCommandsEnum._3;
                                break;
                            case '4':
                                standardCommand = StandardCommandsEnum._4;
                                break;
                            case '5':
                                standardCommand = StandardCommandsEnum._5;
                                break;
                            case '6':
                                standardCommand = StandardCommandsEnum._6;
                                break;
                            case '7':
                                standardCommand = StandardCommandsEnum._7;
                                break;
                            case '8':
                                standardCommand = StandardCommandsEnum._8;
                                break;
                            case '9':
                                standardCommand = StandardCommandsEnum._9;
                                break;
                            case ' ':                 //space
                                if (SupportsDash)
                                {
                                    goto case '-';
                                }
                                else if (SupportsPeriod)
                                {
                                    goto case '.';
                                }
                                break;
                            case '.':
                                if (SupportsPeriod)
                                {
                                    standardCommand = StandardCommandsEnum.Period;
                                }
                                else if (SupportsDash)
                                {
                                    goto case '-';
                                }
                                break;
                            case '-':
                                if (SupportsDash)
                                {
                                    standardCommand = StandardCommandsEnum.Dash;
                                }
                                else if (SupportsPeriod)
                                {
                                    goto case '.';
                                }
                                break;
                            default:
                                continue;
                        }

                        var command = BuildCommand(standardCommand, CommonCommandGroupType.Keypad,
                            CommandPriority.Normal);
                        SendCommand(command);

                        CrestronEnvironment.Sleep((int)_channelSeqConfig.DelayBetweenCommands);
                    }

                    if (_channelSeqConfig.TriggerEnterAfterCommands)
                        Enter();
                }
            }
        }

        protected override void DeConstructChannel(string response)
        {
            response = response.TrimEnd();
            if (response.Contains(" "))
            {
                response = response.Replace(' ', '-');
            }
            if (response.Contains("CH_STATUS"))
            {
                response = response.Replace("CH_STATUS", "");
            }
            base.DeConstructChannel(response);
        }

    }



    public class DelimiterValidator : ResponseValidation
    {
        public DelimiterValidator(DataValidation dataValidation) : base (dataValidation)
        {
            DataValidation = dataValidation;
        }

        public override ValidatedRxData ValidateResponse(string response, CommonCommandGroupType commandGroup)
        {
            var validatedRxData = new ValidatedRxData(false, null);
            const string feedbackParsingValue = "\r";
           
            if (!response.Contains(feedbackParsingValue)) return validatedRxData;

            response = response.Replace(feedbackParsingValue, "");
            validatedRxData.Data = response;
            validatedRxData.Ready = true;

            if (response.Contains(DataValidation.Feedback.ChannelFeedback.GroupHeader))
            {
                validatedRxData.Data = validatedRxData.Data.Remove(0, DataValidation.Feedback.ChannelFeedback.GroupHeader.Length);
                validatedRxData.Data = validatedRxData.Data.Replace(" LOCAL", "").Replace(" REMOTE","");
                validatedRxData.Data = validatedRxData.Data.TrimStart();
                
                var channelNumbers = validatedRxData.Data.Split(' ');
                string major = "";
                string minor = "";

                for (int i = 0; i < channelNumbers.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            major = channelNumbers[i].TrimStart('0');
                            break;
                        case 1:
                            minor = channelNumbers[i].TrimStart('0');
                            break;
                    }
                }
                validatedRxData.Data = string.Format("{0} {1}", major, minor);
             
                validatedRxData.CommandGroup = CommonCommandGroupType.Channel;
            }
            
            return validatedRxData;
        }
    }
}
