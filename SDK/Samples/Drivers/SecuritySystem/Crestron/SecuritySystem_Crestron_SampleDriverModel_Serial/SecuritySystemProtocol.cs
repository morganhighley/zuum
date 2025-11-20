using System;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Transports;
using System.Collections.Generic;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.DeviceTypes.SecuritySystem;
using Crestron.SimplSharp;

namespace SecuritySystem_Crestron_SampleDriverModel_Serial
{
	public class SecuritySystemProtocol : ABaseDriverProtocol, IDisposable
	{
		public SecuritySystemProtocol(ISerialTransport transportDriver, byte id)
			: base(transportDriver, id)
		{

		}

		protected override void ChooseDeconstructMethod(Crestron.RAD.Common.BasicDriver.ValidatedRxData validatedData)
		{
			try
			{
				switch (validatedData.CommandGroup)
				{
					case CommonCommandGroupType.MonitoringAreaInfo:
						DeConstructMonitoringAreaInfo(validatedData.Data);
						break;
					case CommonCommandGroupType.MonitoringAreaResourceStatus:
						DeConstructMonitoringAreaResourceStatus(validatedData.Data);
						break;
					case CommonCommandGroupType.Connection:
						DeConstructConnection(validatedData.Data);
						break;
					case CommonCommandGroupType.MonitoringAreaAlarm:
						DeConstructMonitoringAreaAlarm(validatedData.Data);
						break;
				}
			}
			catch (Exception e)
			{

			}
		}

		protected override void ConnectionChangedEvent(bool connection)
		{
			// PlaceHolder for implementation
		}


		#region Events

		public event StateChangeHandler StateChange;
		public event AlarmChangeHandler AlarmChange;
		public event ErrorChangeHandler ErrorChange;

		#endregion

		#region Property

		public Dictionary<StandardCommandsEnum, string> CommandsDictionary { get; protected internal set; }

		protected bool Connected { get; set; }

		#endregion

		#region Protected

		/// <summary>
		/// Logs timed out message. Sets the Busy flag to false and call SendNextCommand.
		/// </summary>
		protected override void MessageTimedOut(string lastSentCommand)
		{
			LogMessage("SecuritySystemProtocol, MessageTimedOut: " + lastSentCommand);
		}

		protected override void ConnectionChanged(bool connection)
		{
			if (connection == Connected) return;
			Connected = connection;
			base.ConnectionChanged(connection);
		}

		protected void FireEventState(SecuritySystemStateObjects type, object obj)
		{
			var handler = StateChange;
			if (handler != null)
			{
				handler(type, obj);
			}
		}

		protected void FireEventAlarm(object obj)
		{
			var handler = AlarmChange;
			if (handler != null)
			{
				handler(obj);
			}
		}
		protected void FireEventError(object obj)
		{
			var handler = ErrorChange;
			if (handler != null)
			{
				handler(obj);
			}
		}

		protected void UpdateSystemState(SecuritySystemState eventType, bool updatedState)
		{
			var stateObj = new BuiltInSecuritySystemState
			{
				EventType = eventType,
				State = updatedState
			};
			FireEventState(SecuritySystemStateObjects.SecuritySystemStates, stateObj);
		}

		protected void UpdateSystemAlarm(SecuritySystemAlarm alarm, bool updatedState)
		{
			var stateObj = new SecuritySystemAlarmState
			{
				Alarm = alarm,
				State = updatedState
			};
			FireEventAlarm(stateObj);
		}

		protected void UpdateStateError(SecuritySystemError errorType, bool updatedState)
		{
			var stateObj = new SecuritySystemErrorState
			{
				Error = errorType,
				State = updatedState
			};
			FireEventError(stateObj);
		}

		#endregion

		#region Public Method

		public SecuritySystemOperationalResult PrivateDisarmAreas(List<int> areaIndexes, string password)
		{
			var CommandResults = new SecuritySystemOperationalResult(1) { CommandType = SecuritySystemCommandType.Disarm, TargetComponentId = areaIndexes, Result = SecuritySystemOperationalResultCode.Unknown };
			DisarmAreas(areaIndexes, password);
			return CommandResults;
		}

		public virtual void SetSystemState(SecuritySystemSettableStates stateToSet)
		{
			string commandString;
			switch (stateToSet)
			{
				case SecuritySystemSettableStates.ArmAway:
					commandString = CommandsDictionary[StandardCommandsEnum.SetSystemStateToArmAway];
					break;
				case SecuritySystemSettableStates.ArmStay:
					commandString = CommandsDictionary[StandardCommandsEnum.SetSystemStateToArmStay];
					break;
				case SecuritySystemSettableStates.ArmInstant:
					commandString = CommandsDictionary[StandardCommandsEnum.SetSystemStateToArmInstant];
					break;
				case SecuritySystemSettableStates.Disarm:
					commandString = CommandsDictionary[StandardCommandsEnum.SetSystemStateToDisarmed];
					break;
				default:
					return;
			}

			// This is example, Please send the command set as per businees logic
			var command = new CommandSet("SetSystemState" + stateToSet, commandString,
				CommonCommandGroupType.Unknown, null, false,
				CommandPriority.Normal, StandardCommandsEnum.SetSystemStateToArmAway);
			PrepareStringThenSend(command);
		}

		#endregion

		#region CommandSet

		public void SendCommandInternal(CommandSet command)
		{
			PrepareStringThenSend(command);
		}

		#endregion

		#region Logging

		internal void LogMessage(string message)
		{
			if (!EnableLogging) return;

			if (CustomLogger == null)
			{
				CrestronConsole.PrintLine(message);
			}
			else
			{
				CustomLogger(message + "\n");
			}
		}

		#endregion Logging

		#region Private Method

		private void DeConstructMonitoringAreaInfo(string response)
		{
			var state = new List<SecuritySystemState>();
			state.Add(SecuritySystemState.ArmedAway);
			state.Add(SecuritySystemState.Disarmed);

			var alramType = new List<SecuritySystemAlarmType>();
			alramType.Add(SecuritySystemAlarmType.Alarm);
			alramType.Add(SecuritySystemAlarmType.Medical);
			alramType.Add(SecuritySystemAlarmType.Unknown);

			var areaCommand = new List<SecuritySystemAreaCommand>();
			areaCommand.Add(new SecuritySystemAreaCommand(1, SecuritySystemCommandType.Disarm, false));
			areaCommand.Add(new SecuritySystemAreaCommand(2, SecuritySystemCommandType.ForceAway, false));
			areaCommand.Add(new SecuritySystemAreaCommand(3, SecuritySystemCommandType.ForceStay, true));
			var securitySystemArea = new SecuritySystemArea("Area1", 1, state.AsReadOnly(), alramType.AsReadOnly(), areaCommand.AsReadOnly(), this);

			FireEvent(SecuritySystemStateObjects.DeviceSetup, securitySystemArea);
		}


		private void DeConstructMonitoringAreaResourceStatus(string response)
		{
			FireEvent(SecuritySystemStateObjects.SecuritySystemStates, new BuiltInSecuritySystemState() { EventType = SecuritySystemState.Disarmed, State = true });
		}

		private void DeConstructConnection(string response)
		{
			var con = new Connection() { IsConnected = true };
			FireEvent(SecuritySystemStateObjects.Connection, con);
		}

		private void DeConstructMonitoringAreaAlarm(string response)
		{
			if (AlarmChange != null)
			{
				var alram = new SecuritySystemAlarm(SecuritySystemAlarmType.Unknown, false);
				var alarmState = new SecuritySystemAlarmState() { Alarm = alram, State = false };
				AlarmChange.Invoke(alarmState);
			}
		}

		private void FireEvent(SecuritySystemStateObjects type, object obj)
		{
			if (StateChange != null)
			{
				StateChange(type, obj);
			}
		}

		private void DisarmAreas(List<int> areaIndexes, string password)
		{
			CommandSet command = BuildCommand(StandardCommandsEnum.DisarmResource,
				CommonCommandGroupType.Disarm, CommandPriority.High, "Disarm");
			if (command != null)
			{
				SendCommand(command);
			}
		}


		#endregion
	}

	public delegate void StateChangeHandler(SecuritySystemStateObjects securitySystemStateObjects, object changedObject);
	public delegate void AlarmChangeHandler(object changedObject);
	public delegate void ErrorChangeHandler(object changedObject);

	public class RxStatusText
	{
		public String Text { get; set; }
	}

	public class BuiltInSecuritySystemState
	{
		public SecuritySystemState EventType { get; set; }
		public bool State { get; set; }
	}

	public class SecuritySystemErrorState
	{
		public SecuritySystemError Error { get; set; }
		public bool State { get; set; }
	}

	public class SecuritySystemAlarmState
	{
		public SecuritySystemAlarm Alarm { get; set; }
		public bool State { get; set; }
	}

	public class Connection
	{
		public bool IsConnected { get; set; }
	}
}