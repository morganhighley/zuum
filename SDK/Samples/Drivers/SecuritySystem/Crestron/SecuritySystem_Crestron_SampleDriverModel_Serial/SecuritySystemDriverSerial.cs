using System;
using System.Collections.Generic;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.Common.Events;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.DeviceTypes.SecuritySystem;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Crestron.SimplSharp.Reflection;
using Crestron.RAD.Common;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.ProTransports;

namespace SecuritySystem_Crestron_SampleDriverModel_Serial
{
	public class SecuritySystemDriverSerial : ABasicDriver,
		ISecuritySystem,
		IAuthentication3,
		IConnection3, ISerialComport, ISimpl
	{

		#region Ctor

		public SecuritySystemDriverSerial()
			: base()
		{
		}

		#endregion

		#region ISerialComport Members

		public void Initialize(IComPort comPort)
		{
			ConnectionTransport = new CommonSerialComport(comPort)
			{
				EnableLogging = InternalEnableLogging,
				CustomLogger = InternalCustomLogger,
				EnableRxDebug = InternalEnableRxDebug,
				EnableTxDebug = InternalEnableTxDebug
			};


			_securitySystemProtocol = new SecuritySystemProtocol(ConnectionTransport, Id)
			{
				EnableLogging = InternalEnableLogging,
				CustomLogger = InternalCustomLogger
			};

			_securitySystemKeypad = new SecuritySystemKeypad();
			(_securitySystemKeypad as SecuritySystemKeypad).Initialize(_securitySystemProtocol);

			_securitySystemProtocol.StateChange += OnSecuritySystemStateChanged;
			_securitySystemProtocol.RxOut += SendRxOut;
		}

		#endregion

		#region ISimpl

		public SimplTransport Initialize(Action<string, object[]> send)
		{
			_transport = new SimplTransport { Send = send };
            _transport.DriverID = DriverID;
			ConnectionTransport = _transport;

			_securitySystemProtocol = new SecuritySystemProtocol(ConnectionTransport, Id);
			_securitySystemProtocol.StateChange += OnSecuritySystemStateChanged;
			_securitySystemProtocol.RxOut += SendRxOut;

			return _transport;
		}

		#endregion

		#region Fields

		private SimplTransport _transport;
		SecuritySystemProtocol _securitySystemProtocol;
		IEmulatedSecuritySystemKeypad _securitySystemKeypad;
		List<SecuritySystemAlarmType> _supportedAlarms = new List<SecuritySystemAlarmType>();
		List<SecuritySystemState> _supportedStates = new List<SecuritySystemState>();

		List<SecuritySystemError> _currentlyActiveErrors = new List<SecuritySystemError>();
		List<ISecuritySystemZone> _listOfZones = new List<ISecuritySystemZone>();
		List<ISecuritySystemArea> _listOfAreas = new List<ISecuritySystemArea>();
		List<SecuritySystemAreaCommand> _listOfAvailableAreaCommands = new List<SecuritySystemAreaCommand>();
		protected List<SecuritySystemCapabilities> CapabilityList = new List<SecuritySystemCapabilities> { SecuritySystemCapabilities.DirectControl };
		List<SecuritySystemAlarmType> _currentlyActiveAlarms = new List<SecuritySystemAlarmType>();
		List<SecuritySystemState> _currentlyActiveStates = new List<SecuritySystemState>();
		List<SecuritySystemAlarmType> _listOfAvailableAlarms = new List<SecuritySystemAlarmType>();
		List<SecuritySystemState> _listOfAvailableStates = new List<SecuritySystemState>();

		ReadOnlyCollection<SecuritySystemAreaCommand> _availableAreaCommands;
		ReadOnlyCollection<SecuritySystemAlarmType> _availableAlarms;
		ReadOnlyCollection<SecuritySystemState> _availableStates;


		#endregion

		#region Property

		#endregion

		#region ISecuritySystem Implementation

		#region Evetns

		public event EventHandler<ListChangedEventArgs<SecuritySystemAlarmType>> SecuritysystemAlarmStateChangedEvent;
		public event EventHandler<ListChangedEventArgs<ISecuritySystemArea>> SecuritySystemAreaListChanged;
		public event EventHandler<SecuritySystemCommandResultEventArgs> SecuritySystemCommandResult;
		public event EventHandler<ListChangedEventArgs<SecuritySystemError>> SecuritySystemErrorChanged;
		public event EventHandler<ListChangedEventArgs<SecuritySystemState>> SecuritysystemStateChangedEvent;
		public event EventHandler<ListChangedEventArgs<ISecuritySystemZone>> SecuritySystemZoneListChanged;

		#endregion

		public void BypassZone(int areaIndex, int zoneIndex, string password)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<SecuritySystemError> GetActiveErrors()
		{
			return _currentlyActiveErrors;
		}

		public IEnumerable<ISecuritySystemZone> GetAllZones()
		{
			return _listOfZones;
		}

		public ISecuritySystemArea GetArea(int index)
		{
			for (int i = 0; i < _listOfAreas.Count; i++)
			{
				if (_listOfAreas[i].Index == index)
					return _listOfAreas[i];
			}
			return null;
		}

		public IEnumerable<ISecuritySystemArea> GetAreas()
		{
			return _listOfAreas;
		}

		public ReadOnlyCollection<SecuritySystemAreaCommand> GetAvailableAreaCommands()
		{
			return _availableAreaCommands;
		}

		public ReadOnlyCollection<SecuritySystemAlarmType> GetAvailableSystemAlarms()
		{
			return _availableAlarms;
		}

		public ReadOnlyCollection<SecuritySystemState> GetAvailableSystemStates()
		{
			return _availableStates;
		}

		public ReadOnlyCollection<SecuritySystemCapabilities> GetCapabilities()
		{
			return new ReadOnlyCollection<SecuritySystemCapabilities>(CapabilityList);
		}

		public IEnumerable<SecuritySystemAlarmType> GetCurrentSystemAlarms()
		{
			return _currentlyActiveAlarms;
		}

		public IEnumerable<SecuritySystemState> GetCurrentSystemStates()
		{
			return _currentlyActiveStates;
		}

		public IEmulatedSecuritySystemKeypad GetEmulatedKeypad()
		{
			return _securitySystemKeypad;
		}

		public SecuritySystemOperationalResult SendAreaCommand(List<int> areaIndexes, int commandIndex, string password)
		{
			SecuritySystemAreaCommand areaCommand = null;

			if (_availableAreaCommands != null)
			{
				foreach (var command in _availableAreaCommands)
				{
					if (command.Index == commandIndex)
					{
						areaCommand = command;
						break;
					}
				}
			}

			SecuritySystemOperationalResult result = null;
			if (areaCommand != null)
			{
				switch (areaCommand.CommandType)
				{
					case SecuritySystemCommandType.Disarm:
						result = DisarmAreas(areaIndexes, password);
						break;
					default:
						result = new SecuritySystemOperationalResult(0)
						{
							CommandType = SecuritySystemCommandType.Unknown,
							Result = SecuritySystemOperationalResultCode.Success,
							TargetComponentId = new List<int>()
						};
						break;
				}
			}

			return result;
		}


		#endregion

		#region IAuthentication3

		public event EventHandler<AuthenticationEventArgs> IsAuthenticatedChanged;

		#endregion

		#region Base Implementation

		#region JSON Converters

		protected override JsonConverter CreateDeviceSupportConverter()
		{
			return new DeviceSupportConverter();
		}

		#endregion JSON Converters

		public override void ConvertJsonFileToDriverData(string jsonString)
		{
			var obj = JsonConvert.DeserializeObject<BaseRootObject>(jsonString, CreateSerializerSettings());

			BaseRootObject baseRootObject = new BaseRootObject();
			try
			{
				baseRootObject.CrestronSerialDeviceApi = new Crestron.RAD.Common.BasicDriver.CrestronSerialDeviceApi();
				baseRootObject.CrestronSerialDeviceApi.GeneralInformation = obj.CrestronSerialDeviceApi.GeneralInformation;

				baseRootObject.CrestronSerialDeviceApi.Api = new Crestron.RAD.Common.BasicDriver.Api();
				baseRootObject.CrestronSerialDeviceApi.Api.Communication = new Crestron.RAD.Common.Communication();
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.CommunicationType = obj.CrestronSerialDeviceApi.Api.Communication.CommunicationType;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.Protocol = obj.CrestronSerialDeviceApi.Api.Communication.Protocol;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.Baud = obj.CrestronSerialDeviceApi.Api.Communication.Baud;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.Parity = obj.CrestronSerialDeviceApi.Api.Communication.Parity;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.HwHandshake = obj.CrestronSerialDeviceApi.Api.Communication.HwHandshake;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.SwHandshake = obj.CrestronSerialDeviceApi.Api.Communication.SwHandshake;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.DataBits = obj.CrestronSerialDeviceApi.Api.Communication.DataBits;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.StopBits = obj.CrestronSerialDeviceApi.Api.Communication.StopBits;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.Port = obj.CrestronSerialDeviceApi.Api.Communication.Port;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.EnableAutoPolling = obj.CrestronSerialDeviceApi.Api.Communication.EnableAutoPolling;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.EnableAutoReconnect = obj.CrestronSerialDeviceApi.Api.Communication.EnableAutoReconnect;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.TimeBetweenCommands = obj.CrestronSerialDeviceApi.Api.Communication.TimeBetweenCommands;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.ResponseTimeout = obj.CrestronSerialDeviceApi.Api.Communication.ResponseTimeout;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.WaitForResponse = obj.CrestronSerialDeviceApi.Api.Communication.WaitForResponse;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.IpProtocol = obj.CrestronSerialDeviceApi.Api.Communication.IpProtocol;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.IsUserAdjustable = obj.CrestronSerialDeviceApi.Api.Communication.IsUserAdjustable;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.Authentication = obj.CrestronSerialDeviceApi.Api.Communication.Authentication;
				baseRootObject.CrestronSerialDeviceApi.Api.Communication.IsSecure = obj.CrestronSerialDeviceApi.Api.Communication.IsSecure;
				if (obj.CrestronSerialDeviceApi.Api.Communication.UserAdjustableProperties != null)
				{
					baseRootObject.CrestronSerialDeviceApi.Api.Communication.UserAdjustableProperties = new List<string>();
					string supportedEnumString;
					for (int i = 0; i < obj.CrestronSerialDeviceApi.Api.Communication.UserAdjustableProperties.Count; i++)
					{
						supportedEnumString = obj.CrestronSerialDeviceApi.Api.Communication.UserAdjustableProperties[i];
						try
						{
							eTransportAdjustableProperties property = (eTransportAdjustableProperties)Enum.Parse(typeof(eTransportAdjustableProperties), supportedEnumString, true);
							if (Enum.IsDefined(typeof(eTransportAdjustableProperties), property))
							{
								baseRootObject.CrestronSerialDeviceApi.Api.Communication.UserAdjustableProperties.Add(supportedEnumString);
							}
							else
							{
								Log(string.Format("Error parsing TransportAdjustableProperties node in JSON. Item = {0} ", supportedEnumString));
							}
						}
						catch (Exception e)
						{
							Log(string.Format("Error parsing TransportAdjustableProperties node in JSON. Item = {0} Exception = {1}", supportedEnumString, e.Message));
						}
					}
				}

				baseRootObject.CrestronSerialDeviceApi.Api.Communication.DeviceId = obj.CrestronSerialDeviceApi.Api.Communication.DeviceId;
				baseRootObject.CrestronSerialDeviceApi.Api.CustomCommands = obj.CrestronSerialDeviceApi.Api.CustomCommands;
				baseRootObject.CrestronSerialDeviceApi.Api.StandardCommands = obj.CrestronSerialDeviceApi.Api.StandardCommands;

				baseRootObject.CrestronSerialDeviceApi.DeviceSupport = new Dictionary<CommonFeatureSupport, bool>();
				if (obj.CrestronSerialDeviceApi.DeviceSupport != null)
				{
					foreach (var supportedEnum in obj.CrestronSerialDeviceApi.DeviceSupport)
					{
						if (Enum.IsDefined(typeof(CommonFeatureSupport), (int)supportedEnum.Key))
						{
							baseRootObject.CrestronSerialDeviceApi.DeviceSupport.Add((CommonFeatureSupport)supportedEnum.Key, supportedEnum.Value);
						}
					}
				}

				base.Initialize(baseRootObject);
			}
			catch (Exception ex)
			{
				Log(string.Format("SecuritySystem.SDK.ASecuritySystem.ConvertJsonFileToDriverData Error: {0}", ex.Message));
			}

		}

		public override CType AbstractClassType
		{
			get { return GetType(); }
		}


		public override void Dispose()
		{
			if (_securitySystemProtocol != null)
			{
				_securitySystemProtocol.StateChange -= OnSecuritySystemStateChanged;
				_securitySystemProtocol.AlarmChange -= OnSecurityAlarmChanged;
				_securitySystemProtocol.RxOut -= SendRxOut;
			}

			base.Dispose();
		}

		#endregion

		#region Private Method

		private SecuritySystemOperationalResult DisarmAreas(List<int> areaIndexes, string password)
		{
			return _securitySystemProtocol.PrivateDisarmAreas(areaIndexes, password);
		}

		private void UpdateSystemState(SecuritySystemState eventType, bool updatedState)
		{
			if (SecuritysystemStateChangedEvent == null) return;
			if (updatedState)
				_currentlyActiveStates.Add(eventType);
			else
				_currentlyActiveStates.Remove(eventType);

			SecuritysystemStateChangedEvent(this, new ListChangedEventArgs<SecuritySystemState>(ListChangedAction.Reset, eventType, eventType, 0));
		}

		void OnSecuritySystemStateChanged(SecuritySystemStateObjects securitySystemStateObjects, object changedObject)
		{
			switch (securitySystemStateObjects)
			{
				case SecuritySystemStateObjects.DeviceSetup:
					{
						var area = changedObject as ISecuritySystemArea;

						if (area != null)
						{
							_listOfAreas.Add(area);

							if (SecuritySystemAreaListChanged != null)
							{
								var args = new ListChangedEventArgs<ISecuritySystemArea>(
									ListChangedAction.Added, null, area, 1);

								SecuritySystemAreaListChanged.Invoke(this, args);
							}
						}
						break;
					}
				case SecuritySystemStateObjects.Connection:
					var connection = (Connection)changedObject;
					if (Connected == connection.IsConnected) return;
					Connected = connection.IsConnected;
					break;

				case SecuritySystemStateObjects.SecuritySystemStates:
					var newSecuritySystemState = (BuiltInSecuritySystemState)changedObject;
					UpdateSystemState(newSecuritySystemState.EventType, newSecuritySystemState.State);
					break;
			}
		}

		void OnSecurityAlarmChanged(object changedObject)
		{
			var obj = changedObject as SecuritySystemAlarmState;
			if (obj != null && obj.Alarm != null)
			{
				_currentlyActiveAlarms.Add(obj.Alarm.AlarmType);

				if (SecuritysystemAlarmStateChangedEvent != null)
				{
					var args = new ListChangedEventArgs<SecuritySystemAlarmType>(
								   ListChangedAction.Added, SecuritySystemAlarmType.Unknown, obj.Alarm.AlarmType, 1);
					SecuritysystemAlarmStateChangedEvent.Invoke(this, args);
				}
			}
		}

		#endregion
	}
}