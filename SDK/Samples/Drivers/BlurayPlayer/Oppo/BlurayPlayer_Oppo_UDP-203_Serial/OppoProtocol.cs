// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="OppoProtocol.cs">
//   
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------

namespace Crestron.RAD.Drivers.BlurayPlayers
{
    using System;
    using Crestron.SimplSharp;
    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.BlurayPlayer;
    using System.Collections.Generic;
    using Crestron.SimplSharp.Reflection;

    public class OppoProtocol : ABlurayPlayerProtocol
    {
        private const string SvmModeCommand = "#SVM 2\u000D\u000A";

        public OppoProtocol(ISerialTransport transportDriver, byte id)
            : base(transportDriver, id)
        {
            ResponseValidation = new OppoValidator(ValidatedData);
            ValidatedData.PowerOnPollingSequence = new[] {
                StandardCommandsEnum.PowerPoll,
                StandardCommandsEnum.PlayBackStatusPoll, 
                StandardCommandsEnum.TrackPoll, 
                StandardCommandsEnum.ChapterPoll,
                StandardCommandsEnum.TrackElapsedTimePoll,  
                StandardCommandsEnum.TrackRemainingTimePoll, 
                StandardCommandsEnum.ChapterElapsedTimePoll, 
                StandardCommandsEnum.ChapterRemainingTimePoll, 
                StandardCommandsEnum.TotalElapsedTimePoll,  
                StandardCommandsEnum.TotalRemainingTimePoll
            };

            PollingInterval = 3000;
        }

        protected override void ConnectionChanged(bool connection)
        {
            if (connection)
            {
                Transport.Send(SvmModeCommand, null);
            }
            //Make sure the event gets sent up to UI
            base.ConnectionChanged(connection);
        }

        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            if (!commandSet.CommandPrepared)
            {
                commandSet.Command = string.Format("#{0}\u000D\u000A", commandSet.Command);
            }
            return base.PrepareStringThenSend(commandSet);
        }
    }

    public class OppoValidator : ResponseValidation
    {
        private const string StartOfResponse = "@";
        private const string EndOfResponse = "\u000D";

        public OppoValidator(DataValidation dataValidation)
            : base(dataValidation)
        {
            DataValidation = dataValidation;
        }

        private ValidatedRxData GenerateValidatedData(CommonCommandGroupType group, string response)
        {
            ValidatedRxData data = new ValidatedRxData(false, string.Empty);

            var header = string.Empty;
            switch (group)
            {
                case CommonCommandGroupType.Power:
                    header = DataValidation.Feedback.PowerFeedback.GroupHeader;
                    break;
                case CommonCommandGroupType.PlayBackStatus:
                    header = DataValidation.Feedback.PlayBackStatusFeedback.GroupHeader;
                    break;
                case CommonCommandGroupType.ChapterElapsedTime:
                    header = DataValidation.Feedback.ChapterElapsedTimeFeedback.GroupHeader;
                    break;
                case CommonCommandGroupType.ChapterFeedback:
                    header = DataValidation.Feedback.ChapterFeedback.GroupHeader;
                    break;
                case CommonCommandGroupType.ChapterRemainingTime:
                    header = DataValidation.Feedback.ChapterRemainingTimeFeedback.GroupHeader;
                    break;
                case CommonCommandGroupType.TotalElapsedTime:
                    header = DataValidation.Feedback.TotalElapsedTimeFeedback.GroupHeader;
                    break;
                case CommonCommandGroupType.TotalRemainingTime:
                    header = DataValidation.Feedback.TotalRemainingTimeFeedback.GroupHeader;
                    break;
                case CommonCommandGroupType.TrackElapsedTime:
                    header = DataValidation.Feedback.TrackElapsedTimeFeedback.GroupHeader;
                    break;
                case CommonCommandGroupType.TrackFeedback:
                    header = DataValidation.Feedback.TrackFeedback.GroupHeader;
                    break;
                case CommonCommandGroupType.TrackRemainingTime:
                    header = DataValidation.Feedback.TrackRemainingTimeFeedback.GroupHeader;
                    break;
            }
            if (header.Equals(string.Empty))
            {
                data.Ignore = true;
            }
            else
            {
                response = response.Replace(header, string.Empty);
                response = response.Replace("OK", string.Empty);
                response = response.Replace(" ", string.Empty);
                response = response.Replace("\u000D", string.Empty);

                data.CommandGroup = group;
                data.Data = response;
                data.Ready = true;
                }
            return data;
        }

        public override ValidatedRxData ValidateResponse(string response, CommonCommandGroupType commandGroup)
        {
            var validatedData = new ValidatedRxData(false, string.Empty);

            
            if (response.StartsWith(StartOfResponse) && response.EndsWith(EndOfResponse))
            {
                //if (!string.IsNullOrEmpty(response))
                //{
                //   CrestronConsole.PrintLine("Rcvd response:{0}:", response);
                //}

                var feedbackGroup = DataValidation.Feedback;
                response = response.Replace(StartOfResponse, string.Empty);

                if (((response.IndexOf("ER INVALID", System.StringComparison.InvariantCultureIgnoreCase)) >= 0) ||
                    ((response.IndexOf("ER BUSY", System.StringComparison.InvariantCultureIgnoreCase)) >= 0))
                {
                    validatedData.Ignore = true;
                }
                else if (response.Equals("OK OFF" + EndOfResponse)) //Added because the Oppo omits the QPW in response when short Power cycle occurs. Bug#156260 - RCO 
                {
                    validatedData.CommandGroup = CommonCommandGroupType.Power;
                    validatedData.Data = DataValidation.PowerFeedback.Feedback[StandardFeedback.PowerStatesFeedback.Off];
                    validatedData.Ready = true;
                }
                else if (response.Equals("OK ON" + EndOfResponse))//Added because the Oppo omits the QPW in response when short Power cycle occurs. Bug#156260 - RCO 
                {
                    validatedData.CommandGroup = CommonCommandGroupType.Power;
                    validatedData.Data = DataValidation.PowerFeedback.Feedback[StandardFeedback.PowerStatesFeedback.On];
                    validatedData.Ready = true;
                }


                else if (response.StartsWith(feedbackGroup.PowerFeedback.GroupHeader))
                {
                    validatedData = GenerateValidatedData(CommonCommandGroupType.Power, response);
                }
                else if (response.StartsWith(feedbackGroup.PlayBackStatusFeedback.GroupHeader))
                {
                    validatedData = GenerateValidatedData(CommonCommandGroupType.PlayBackStatus, response);
                }
                else if (response.StartsWith(feedbackGroup.ChapterElapsedTimeFeedback.GroupHeader))
                {
                    validatedData = GenerateValidatedData(CommonCommandGroupType.ChapterElapsedTime, response);
                }
                else if (response.StartsWith(feedbackGroup.ChapterFeedback.GroupHeader))
                {
                    validatedData = GenerateValidatedData(CommonCommandGroupType.ChapterFeedback, response);
                }
                else if (response.StartsWith(feedbackGroup.ChapterRemainingTimeFeedback.GroupHeader))
                {
                    validatedData = GenerateValidatedData(CommonCommandGroupType.ChapterRemainingTime, response);
                }
                else if (response.StartsWith(feedbackGroup.TotalElapsedTimeFeedback.GroupHeader))
                {
                    validatedData = GenerateValidatedData(CommonCommandGroupType.TotalElapsedTime, response);
                }
                else if (response.StartsWith(feedbackGroup.TotalRemainingTimeFeedback.GroupHeader))
                {
                    validatedData = GenerateValidatedData(CommonCommandGroupType.TotalRemainingTime, response);
                }
                else if (response.StartsWith(feedbackGroup.TrackElapsedTimeFeedback.GroupHeader))
                {
                    validatedData = GenerateValidatedData(CommonCommandGroupType.TrackElapsedTime, response);
                }
                else if (response.StartsWith(feedbackGroup.TrackFeedback.GroupHeader))
                {
                    validatedData = GenerateValidatedData(CommonCommandGroupType.TrackFeedback, response);
                }
                else if (response.StartsWith(feedbackGroup.TrackRemainingTimeFeedback.GroupHeader))
                {
                    validatedData = GenerateValidatedData(CommonCommandGroupType.TrackRemainingTime, response);
                }
                else
                {
                    validatedData.Ignore = true;
                }
            }
            else if (response.EndsWith(EndOfResponse))
            {
                validatedData.Ignore = true;
            }
            return validatedData;
        }
    }
}
