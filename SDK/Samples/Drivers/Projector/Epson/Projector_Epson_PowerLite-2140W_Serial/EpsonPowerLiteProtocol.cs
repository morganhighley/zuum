// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EpsonPowerLiteProtocol.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Crestron.SimplSharp;

namespace Crestron.RAD.Drivers.Displays
{
    using System;

    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.Display;
    using System.Collections.Generic;

    public class EpsonPowerLiteProtocol : ADisplayProtocol
    {
        private const string _standbyNetworkOffResponse = "00";
        private const string _powerOnResponse = "01";
        private const string _warmingUpResponse = "02";
        private const string _coolDownResponse = "03";
        private const string _standbyNetworkOnResponse = "04";
        private const int FIFTEEN_MINUTES = 900000;
        private Dictionary<string, Func<string>> _deconstructPowerOption;
        Stopwatch _powerWaitPeriodStopwatch = new Stopwatch();
        //private CTimer _lampHourPollTimer;

        public EpsonPowerLiteProtocol(ISerialTransport transportDriver, byte id)
            : base(transportDriver, id)
        {
            ResponseValidation = new ResponseValidator(Id, ValidatedData);
            ValidatedData.PowerOnPollingSequence = new[] 
            { 
                StandardCommandsEnum.PowerPoll, 
                StandardCommandsEnum.VideoMutePoll, 
                StandardCommandsEnum.LampHoursPoll, 
                StandardCommandsEnum.VolumePoll,
                StandardCommandsEnum.InputPoll
            };
            //_lampHourPollTimer = new CTimer(PollLampHour, 1000);
            _deconstructPowerOption = new Dictionary<string, Func<string>>();
            _deconstructPowerOption.Add(_standbyNetworkOffResponse, HandleStandbyResponse);
            _deconstructPowerOption.Add(_powerOnResponse, HandlePowerOnResponse);
            _deconstructPowerOption.Add(_warmingUpResponse, HandleWarmupResponse);
            _deconstructPowerOption.Add(_coolDownResponse, HandleCoolDownResponse);
            _deconstructPowerOption.Add(_standbyNetworkOnResponse, HandleStandbyResponse);
        }

        /*public void PollLampHour(object userSpecified)
        {
            try
            {
                this.LampHoursPoll();
            }
            catch
            {

            }
            finally
            {
                _lampHourPollTimer.Reset(FIFTEEN_MINUTES);
            }
        }*/

        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            // Append <CR> to all commands being sent out
            commandSet.Command += "\u000D";
            return base.PrepareStringThenSend(commandSet);
        }

        /*protected override void DeConstructLampHours(string response)
        {
            if (response.Equals("LAMP=00000"))
            {
                
            }
            base.DeConstructLampHours(response);
        }*/

        // Display will tell us when it is cooling down or warming up. 
        // Treat these as Power On and Power Off feedback
        protected override void DeConstructPower(string response)
        {
            if (_deconstructPowerOption.ContainsKey(response))
            {
                response = _deconstructPowerOption[response].Invoke();
                base.DeConstructPower(response);
            }
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
    }

    public class ResponseValidator : ResponseValidation
    {
        private const string _networkOnStandbyResponse = "04";
        private const int _volumeMultiplier = 12;

        public ResponseValidator(byte id, DataValidation dataValidation)
            : base(id, dataValidation)
        {
            Id = id;
            DataValidation = dataValidation;
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

            // 2 = <CR> + ":"
            if (response.EndsWith(":") && response.Length > 2)
            {
                // Ignore the IMEVENT packets
                if (response.Contains("IMEVENT"))
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
                else if (response.Contains(DataValidation.PowerFeedback.GroupHeader) && response.Contains(_networkOnStandbyResponse))
                {
                    validatedData.Data = RemoveHeader(response, DataValidation.PowerFeedback.GroupHeader);
                    validatedData.CommandGroup = CommonCommandGroupType.Power;
                    validatedData.Ready = true;
                    return validatedData;
                }

                // Everything else can be handled by the base
                return base.ValidateResponse(response, commandGroup);
            }
            return new ValidatedRxData(false, string.Empty);
        }
    }
}

