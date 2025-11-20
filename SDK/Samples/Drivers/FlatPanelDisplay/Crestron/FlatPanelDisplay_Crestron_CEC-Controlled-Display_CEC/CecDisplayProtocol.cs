// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanasonicPTRZ570Protocol.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Crestron.RAD.Drivers.Displays
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.Display;
    using Crestron.SimplSharp;

    public class CecDisplayProtocol : ADisplayProtocol
    {
        private static string _cecReleaseCommand = "\u0040\u0045";


        private CommandSet _powerOffRcp;
        private CommandSet _powerOffSs;
        private CommandSet _powerOnRcp;
        private CommandSet _powerOnSas;
        private CommandSet _powerOnOtp;

        public CecDisplayProtocol(ISerialTransport transportDriver)
            : base(transportDriver, 0x00)
        {
            ResponseValidation = new ResponseValidator(Id, ValidatedData);

            // Always act as if the driver is connected to keep the queue from removing anything
            IsConnected = true;
            ConnectionChanged(true);

            _powerOffRcp = new CommandSet(
                "PowerOff RCP",
                "\\u0040\\u0044\\u006C\\u0040\\u0045",
                CommonCommandGroupType.Unknown,
                null,
                true,
                CommandPriority.Highest,
                StandardCommandsEnum.NotAStandardCommand);

            _powerOffSs = new CommandSet(
                "PowerOff SS",
                "\\u0040\\u0036",
                CommonCommandGroupType.Unknown,
                null,
                true,
                CommandPriority.Highest,
                StandardCommandsEnum.NotAStandardCommand);

            _powerOnRcp = new CommandSet(
                "PowerOn RCP",
                "\\u0040\\u0044\\u006D\\u0040\\u0045",
                CommonCommandGroupType.Unknown,
                null,
                true,
                CommandPriority.Highest,
                StandardCommandsEnum.NotAStandardCommand);

            _powerOnSas = new CommandSet(
                "PowerOn SAS",
                "\\u0040\\u0004",
                CommonCommandGroupType.Unknown,
                null,
                true,
                CommandPriority.Highest,
                StandardCommandsEnum.NotAStandardCommand);

            _powerOnOtp = new CommandSet(
                "PowerOn OTP",
                "\\u004F\\u0086\\u0010\\u0000",
                CommonCommandGroupType.Unknown,
                null,
                true,
                CommandPriority.Highest,
                StandardCommandsEnum.NotAStandardCommand);
        }

        /// <summary>
        /// Always show connected
        /// </summary>
        protected override void ConnectionChanged(bool connection)
        {
            connection = true;
            base.ConnectionChanged(connection);
        }

        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            if (commandSet.CommandPrepared)
                return base.PrepareStringThenSend(commandSet);

            if (commandSet.Command.StartsWith("\u0040\u0044") &&
                commandSet.CommandGroup != CommonCommandGroupType.Volume)
            {
                // Send release on all commands that need it, except volume which is handled
                // in ReleaseVolume
                commandSet.CallBack = GenericSendReleaseCallback;
            }

            return base.PrepareStringThenSend(commandSet);
        }

        private void GenericSendReleaseCallback()
        {
            Transport.Send(_cecReleaseCommand, null);
        }

        /// <summary>
        /// This will send 2 different Power off commands twice
        /// </summary>
        public override void PowerOff()
        {
            if (EnableLogging)
                Log("PowerOff Invoked");

            SendCommand(_powerOffRcp);
            SendCommand(_powerOffSs);

            SendCommand(_powerOffRcp);
            SendCommand(_powerOffSs);
        }

        /// <summary>
        /// This will send 3 different Power on commands twice
        /// </summary>
        public override void PowerOn()
        {
            if (EnableLogging)
                Log("PowerOn Invoked");

            SendCommand(_powerOnRcp);
            SendCommand(_powerOnOtp);
            SendCommand(_powerOnSas);

            SendCommand(_powerOnRcp);
            SendCommand(_powerOnOtp);
            SendCommand(_powerOnSas);
        }

        public override void ReleaseVolume()
        {
            base.ReleaseVolume();
            Transport.Send(_cecReleaseCommand, null);
        }
    }

    public class ResponseValidator : ResponseValidation
    {
        public ResponseValidator(byte id, DataValidation dataValidation)
            : base(id, dataValidation)
        {
            Id = id;
            DataValidation = dataValidation;
        }

        public override ValidatedRxData ValidateResponse(string response, CommonCommandGroupType commandGroup)
        {
            return new ValidatedRxData(true, string.Empty)
            {
                CommandGroup = CommonCommandGroupType.AckNak
            };
        }
    }
}

