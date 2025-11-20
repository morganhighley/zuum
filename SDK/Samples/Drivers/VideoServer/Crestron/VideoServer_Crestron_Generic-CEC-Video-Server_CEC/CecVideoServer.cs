// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="CecDisplay.cs">
//   
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------

namespace Crestron.RAD.Drivers.VideoServers
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Crestron.RAD.Common;
    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.VideoServer;
    using Crestron.SimplSharp;
    using Crestron.RAD.Common.ExtensionMethods;

    using Newtonsoft.Json;

    public class CecVideoServer : ABasicVideoServer, ICecDevice
    {
        public CecVideoServer()
        {
        }

        public void Initialize(ISerialTransport transport)
        {
            ConnectionTransport = transport;

            VideoServerProtocol = new CecVideoServerProtocol(transport)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            VideoServerProtocol.StateChange += StateChange;
            VideoServerProtocol.RxOut += SendRxOut;
            VideoServerProtocol.Initialize(VideoServerData);
        }

        /*public override void Connect()
        {
            (VideoServerProtocol as CecVideoServerProtocol).Initialized = false;
            base.Connect();
        }*/

        public SimplTransport Initialize(int id, Action<string, object[]> send)
        {
            var simplTransport = new SimplTransport { Send = send };
            ConnectionTransport = simplTransport;

            VideoServerProtocol = new CecVideoServerProtocol(simplTransport)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            VideoServerProtocol.StateChange += StateChange;
            VideoServerProtocol.RxOut += SendRxOut;
            VideoServerProtocol.Initialize(VideoServerData);

            return simplTransport;
        }
    }
}
