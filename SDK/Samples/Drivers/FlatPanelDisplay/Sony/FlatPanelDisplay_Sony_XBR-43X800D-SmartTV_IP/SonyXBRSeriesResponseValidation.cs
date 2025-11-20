using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.RAD.DeviceTypes.Display;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Enums;

namespace Crestron.RAD.Drivers.Displays
{
    public class SonyXBRSeriesResponseValidation : ResponseValidation
    {
        public SonyXBRSeriesResponseValidation(byte id, DataValidation dataValidation)
            : base(id, dataValidation)
        {
            Id = id;
            DataValidation = dataValidation;
        }

        public override ValidatedRxData ValidateResponse(string response, CommonCommandGroupType commandGroup)
        {
            ValidatedRxData validatedRxData = new ValidatedRxData(false, null);

            

            return validatedRxData;
        }
    }
}