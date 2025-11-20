// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="CecDisplay.cs">
//   
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------

namespace Crestron.RAD.Drivers.Displays
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Crestron.RAD.Common;
    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.Display;
    using Crestron.SimplSharp;
    using Crestron.RAD.Common.ExtensionMethods;

    using Newtonsoft.Json;

    public class CecDisplay : ABasicVideoDisplay, ICecDevice
    {
        public CecDisplay()
        {
        }

        public void Initialize(ISerialTransport transport)
        {
            ConnectionTransport = transport;

            DisplayProtocol = new CecDisplayProtocol(transport)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            DisplayProtocol.StateChange += StateChange;
            DisplayProtocol.RxOut += SendRxOut;
            DisplayProtocol.Initialize(DisplayData);
        }

        public SimplTransport Initialize(int id, Action<string, object[]> send)
        {
            var simplTransport = new SimplTransport { Send = send };
            ConnectionTransport = simplTransport;

            DisplayProtocol = new CecDisplayProtocol(simplTransport)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            DisplayProtocol.StateChange += StateChange;
            DisplayProtocol.RxOut += SendRxOut;
            DisplayProtocol.Initialize(DisplayData);

            return simplTransport;
        }

        
    }
}
