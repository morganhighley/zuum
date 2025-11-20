// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="OppoUdp103Comport.cs">
//   
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------

using Crestron.RAD.DeviceTypes.BlurayPlayer;
namespace Crestron.RAD.Drivers.BlurayPlayers
{
    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.ProTransports;

    public class OppoUdp203Comport : ABasicBlurayPlayer, ISerialComport, ISimpl
    {
        public void Initialize(IComPort comPort)
        {
            ConnectionTransport = new CommonSerialComport(comPort)
            {
                EnableLogging = InternalEnableLogging,
                EnableTxDebug = InternalEnableTxDebug,
                EnableRxDebug = InternalEnableRxDebug,
                CustomLogger = InternalCustomLogger
            };

            BlurayPlayerProtocol = new OppoProtocol(ConnectionTransport, Id);
            BlurayPlayerProtocol.StateChange += StateChange;
            BlurayPlayerProtocol.RxOut += SendRxOut;
            BlurayPlayerProtocol.Initialize(BlurayPlayerData);
        }

        public SimplTransport Initialize(System.Action<string, object[]> send)
        {
            ConnectionTransport = new SimplTransport { Send = send };

            BlurayPlayerProtocol = new OppoProtocol(ConnectionTransport, Id);
            BlurayPlayerProtocol.StateChange += StateChange;
            BlurayPlayerProtocol.RxOut += SendRxOut;
            BlurayPlayerProtocol.Initialize(BlurayPlayerData);

            return ConnectionTransport as SimplTransport;
        }
    }
}
