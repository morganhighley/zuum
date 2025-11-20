// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EpsonPowerLiteTcp.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// For Basic SIMPL# Classes

// For Basic SIMPL#Pro classes

using Crestron.RAD.DeviceTypes.Display;

namespace Crestron.RAD.Drivers.Displays
{
    using Crestron.RAD.Common.Interfaces;
    using Crestron.RAD.Common.Transports;
    using Crestron.SimplSharp;
    using Crestron.SimplSharp.Reflection;
    using System.Linq;
    using Crestron.RAD.Common.Enums;
    using System;
    using Crestron.RAD.Common.BasicDriver;

    public class EpsonPowerLiteTcp : ABasicVideoDisplay, ITcp
    {
        public EpsonPowerLiteTcp()
        {
            _password = string.Empty;
            _passwordKey = string.Empty;

            try
            {
                // Any logic that references capabilities/new features within the constructor must be in a 
                // seperate method for this try/catch to catch the exception if this assembly is loaded
                // on a system without these references.
                AddCapabilities();
            }
            catch (TypeLoadException)
            {
                // This exception would happen if this driver was loaded on a system
                // running RADCommon without ITcp2 / ICapability.
            }
        }

        private void AddCapabilities()
        {
            // Adds the Tcp2 capability to allow applications to use a hostname when
            // initializing the driver.
            var tcp2Capability = new Tcp2Capability(Initialize);
            Capabilities.RegisterInterface(typeof(ITcp2), tcp2Capability);
        }


        public void Initialize(IPAddress ipAddress, int port)
        {
            var tcpTransport = new TcpTransport
            {
                EnableAutoReconnect = EnableAutoReconnect,
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };

            tcpTransport.Initialize(ipAddress, port);
            ConnectionTransport = tcpTransport;

            DisplayProtocol = new EpsonPowerLiteProtocol(ConnectionTransport, Id, this)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            DisplayProtocol.StateChange += StateChange;
            DisplayProtocol.RxOut += SendRxOut;
            DisplayProtocol.Initialize(DisplayData);
        }

        private void Initialize(string address, int port)
        {
            var tcpTransport = new TcpTransport
            {
                EnableAutoReconnect = EnableAutoReconnect,
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };

            tcpTransport.Initialize(address, port);
            ConnectionTransport = tcpTransport;

            DisplayProtocol = new EpsonPowerLiteProtocol(ConnectionTransport, Id, this)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };

            DisplayProtocol.StateChange += StateChange;
            DisplayProtocol.RxOut += SendRxOut;
            DisplayProtocol.Initialize(DisplayData);
        }

        internal string _passwordKey;
        public override string PasswordKey
        {
            set
            {
                _passwordKey = value;
                base.PasswordKey = value;
            }
        }

        internal string _password;
       public override void OverridePassword(string password)
        {
            _password = password;
        }
    }
}

