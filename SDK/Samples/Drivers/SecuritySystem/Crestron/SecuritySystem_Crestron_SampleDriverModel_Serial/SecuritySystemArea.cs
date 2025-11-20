// For Basic SIMPL# Classes
// For Basic SIMPL#Pro classes

using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.RAD.Common.Interfaces;
using System.Collections.ObjectModel;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.DeviceTypes.SecuritySystem;
using Crestron.RAD.Common.Events;

namespace SecuritySystem_Crestron_SampleDriverModel_Serial
{
	/// <summary>
	/// This class is used to implement the security system's area interface
	/// </summary>
	public class SecuritySystemArea : ISecuritySystemArea
	{
		#region Fields

		internal bool AreaInExitDelay = false;
		public readonly List<SecuritySystemState> _activeStates;
		public readonly List<SecuritySystemAlarmType> _activeAlarmTypes;
		public readonly List<SecuritySystemAreaCommand> _availableAreaCommands;
		public readonly List<KeyValuePair<int, ISecuritySystemZone>> _zones;
		readonly ReadOnlyCollection<SecuritySystemState> _supportedArmingStates;
		readonly ReadOnlyCollection<SecuritySystemAlarmType> _supportedAlarmTypes;
		public SecuritySystemOperationalResult _securitySystemOperationalResult;
		private List<SecuritySystemState> _activeAreaStates;
		private List<SecuritySystemAlarmType> _activeAreaAlarms;
		public string SubType { get; protected set; }
		private Dictionary<int, ISecuritySystemZone> _zonesInArea;
		readonly SecuritySystemProtocol _securitySystemProtocol;

		#endregion

		#region Ctor

		public SecuritySystemArea()
		{

		}

		public SecuritySystemArea(string name, int index,
			ReadOnlyCollection<SecuritySystemState> supportedArmingStates,
			ReadOnlyCollection<SecuritySystemAlarmType> supportedAlarmTypes,
			ReadOnlyCollection<SecuritySystemAreaCommand> areaCommands,
		   SecuritySystemProtocol protocol)
		{
			_activeAreaStates = new List<SecuritySystemState>();
			_activeAreaAlarms = new List<SecuritySystemAlarmType>();
			_securitySystemProtocol = protocol;
			_supportedArmingStates = supportedArmingStates;
			_supportedAlarmTypes = supportedAlarmTypes;
			Name = name;
			Index = index;
			_zonesInArea = new Dictionary<int, ISecuritySystemZone>();
		}

		#endregion

		#region ISecuritySystemArea Implementation

		public int Index { get; set; }
		public string Name { get; set; }
		public event EventHandler<ValueEventArgs<string>> NameChanged;
		public event EventHandler<ListChangedEventArgs<SecuritySystemAlarmType>> SecuritysystemAlarmStateChangedEvent;
		public event EventHandler<ListChangedEventArgs<SecuritySystemState>> SecuritysystemAreaStateChangedEvent;
		public event EventHandler<ListChangedEventArgs<ISecuritySystemZone>> ZoneListChangedEvent;

		public IEnumerable<SecuritySystemAlarmType> GetActiveAlarms()
		{
			return _activeAreaAlarms;
		}

		public IEnumerable<SecuritySystemState> GetActiveStates()
		{
			return _activeAreaStates;
		}

		public ReadOnlyCollection<SecuritySystemAlarmType> GetSupportedAlarmTypes()
		{
			return _supportedAlarmTypes;
		}

		public ReadOnlyCollection<SecuritySystemState> GetSupportedArmingStates()
		{
			return _supportedArmingStates;
		}

		public IEnumerable<KeyValuePair<int, ISecuritySystemZone>> GetZones()
		{
			return _zonesInArea;
		}

		public SecuritySystemOperationalResult SendAreaCommand(int commandIndex, string password)
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
			var areaIndexes = new List<int>() { this.Index };

			SecuritySystemOperationalResult result = null;
			if (areaCommand != null)
			{
				switch (areaCommand.CommandType)
				{
					case SecuritySystemCommandType.Disarm:
						result = _securitySystemProtocol.PrivateDisarmAreas(areaIndexes, password);
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

		#region Public

		public void Log(string message)
		{
			message = string.Format("{0}::({1}) {2} : {3}", CrestronEnvironment.TickCount, _securitySystemProtocol.DriverID, GetType().Name, message);
			CrestronConsole.PrintLine(message);
		}

		#endregion

	}    
}

