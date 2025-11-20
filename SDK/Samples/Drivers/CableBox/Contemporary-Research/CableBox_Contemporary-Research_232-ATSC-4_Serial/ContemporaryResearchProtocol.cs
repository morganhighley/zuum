// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="ContemporaryResearchProtocol.cs">
//   
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------

namespace Crestron.RAD.Drivers.CableBoxes
{
    using System;

    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.CableBox;

    public class ContemporaryResearchProtocol : ACableBoxProtocol
    {
        public ContemporaryResearchProtocol(ISerialTransport transportDriver, byte id)
            : base(transportDriver, id)
        {
            ResponseValidation = new DelimiterValidator(ValidatedData);

            ValidatedData.PowerOnPollingSequence = new[] { StandardCommandsEnum.PowerPoll, StandardCommandsEnum.ChannelPoll, StandardCommandsEnum.VolumePoll, StandardCommandsEnum.MutePoll };                   
        }

        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            if (commandSet.CommandPrepared)
            {
                return base.PrepareStringThenSend(commandSet);
            }

            if (commandSet.StandardCommand.Equals(StandardCommandsEnum.Channel))
            {
                commandSet.Command = commandSet.Command.Replace(".", "-");
            }

            commandSet.Command = String.Format("{0}{1}\x0D", SendHeader, commandSet.Command);
            return base.PrepareStringThenSend(commandSet);
        }

        public override void DataHandler(string rx)
        {
            base.DataHandler(rx);

            // Packets for this device contain multiple feedback values and must be handled differently
            if (!RxData.ToString().Contains("\r\n"))
            {
                return;
            }

            string response = RxData.ToString();

            if (!response.Contains(ReceiveHeader))
            {
                return;
            }

            // Update the connection Status if the current state is set to offline.
            if (!IsConnected)
            {
                IsConnected = true;

                var stateObj = new Connection { IsConnected = IsConnected };
                FireEvent(CableBoxStateObjects.Connection, stateObj);
            }

           // Remove header
            response = response.Remove(0, 1);
            
            // Remove ID
            response = response.Remove(0, 1);
            char responseType = response[0];

            // Remove response type
            response = response.Remove(0, 1);

            switch (responseType)
            {
                case 'V':   //Contains Power State, Volume Level, and Mute Setting
                    MuteIsOn = response[3] == 'U' ? false : true;
                    PowerIsOn = response[0] == 'U' ? true : false;
                    UnscaledVolumeIs = Convert.ToUInt32(String.Format("{0}{1}{2}", response[5], response[6], response[7]));
                    UnscaledRampingVolumeIs = VolumeIs;

                    var powerObj = new Power { PowerIsOn = PowerIsOn, WarmingUp = WarmingUp, CoolingDown = CoolingDown };
                    FireEvent(CableBoxStateObjects.Power, powerObj);

                    var audioObj = new Audio { VolumeIs = VolumeIs, MuteIsOn = MuteIsOn };
                    FireEvent(CableBoxStateObjects.Audio, audioObj);
                    
                    break;
                case 'T':   //Contains Power State & Channel Number.
                    PowerIsOn = response[0] == 'U' ? true : false;

                    string majorChannel = String.Format("{0}{1}{2}", response[1], response[2], response[3]);
                    string minorChannel = String.Format("{0}{1}{2}", response[8], response[9], response[10]);

                    ChannelIs = String.Format("{0}-{1}", majorChannel, minorChannel);

                    var channelObj = new Channel { ChannelIs = ChannelIs };
                    FireEvent(CableBoxStateObjects.Channel, channelObj);

                    powerObj = new Power { PowerIsOn = PowerIsOn, WarmingUp = WarmingUp, CoolingDown = CoolingDown };
                    FireEvent(CableBoxStateObjects.Power, powerObj);

                    break;
            }

            // Clear flag and reset count since command is valid.
            PartialOrUnrecognizedCommand = false;
            PartialOrUnrecognizedCommandCount = 0;
            TimeoutCount = 0;

            RxData.Length = 0;
            // Setting the capacity is a problem on Mono systems.  Over time it
            // takes longer and longer.  It is not necessary anyway since it
            // will free the memory buffer and force it to be re-allocated.
            // RxData.Capacity = 0;
        }

        private const string SendHeader = ">";
        private const string ReceiveHeader = "<";
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
            
            if (!response.Contains(feedbackParsingValue))
            {
                return validatedRxData;
            }
            
            return validatedRxData;
        }
    }
}
