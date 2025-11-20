using System;
using System.Collections.Generic;
using Crestron.RAD.Common.Attributes.Programming;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.Common.Interfaces.ExtensionDevice;
using Crestron.RAD.DeviceTypes.ExtensionDevice;
using Crestron.SimplSharpPro.CrestronThread;
using ExtensionDevice_Crestron_Sample_DoorLock_CloudConnected.Emulator;
using LockState = ExtensionDevice_Crestron_Sample_DoorLock_CloudConnected.Emulator.LockState;

namespace ExtensionDevice_Crestron_Sample_DoorLock_CloudConnected
{
	public class DoorLock : AExtensionDevice, ICloudConnected
	{
		#region Constants

		#region Commands

		// Define all of the keys to be used as commands, these match the keys in the ui definition for command actions
		private const string LockCommand = "Lock";
		private const string UnlockCommand = "Unlock";
		private const string ToggleCommand = "ToggleLock";
		private const string ChangeMasterPinCommand = "ChangeMasterPin";
		private const string AddUserCommand = "AddUser";
		private const string DeleteUserCommand = "DeleteUser";

		#endregion

		#region Property Keys

		// Define all of the keys for properties, these match the properties used in the ui definition
		private const string LockStateIconKey = "LockStateIcon";
		private const string LockedIcon = "icLocksOn";
		private const string UnlockedIcon = "icLocksOff";
		private const string JammedIcon = "icLocksUnknownDisabled";
		private const string SpinnerIcon = "icSpinner";
		private const string LockStateKey = "LockState";
		private const string LockedLabel = "^LockedLabel";
		private const string LockingLabel = "^LockingLabel";
		private const string UnlockedLabel = "^UnlockedLabel";
		private const string UnlockingLabel = "^UnlockingLabel";
		private const string JammedLabel = "^JammedLabel";

		private const string BatteryIconKey = "BatteryIcon";
		private const string BatteryLowIcon = "icBatteryLow";
		private const string BatteryNormalIcon = "";
		private const string BatteryPercentKey = "BatteryPercent";
		private const string BatteryLowLabel = "^BatteryLowLabel";

		private const string MasterPinKey = "MasterPin";
		private const string AutoLockKey = "AutoLock";
		private const string AutoLockTimeKey = "AutoLockTime";

		private const string UserListKey = "UserList";
		private const string UserKey = "User";
		private const string UserNameKey = "Name";
		private const string UserIdKey = "Id";
		private const string UserPinKey = "Pin";
		private const string UserEnabledKey = "Enabled";
		private const string UserPermissionKey = "PermissionLevel";
		private const string AdminLabel = "Admin";
		private const string ResidentLabel = "Resident";
		private const string GuestLabel = "Guest";

		private const string LockActivityFeedKey = "ActivityFeed";
		private const string LockActivityKey = "LockActivity";
		private const string ActivityUserNameKey = "UserName";
		private const string ActivityEventTypeKey = "EventType";
		private const string ActivitySuccessKey = "Success";
		private const string ActivityTimeKey = "Time";
		private const string ActivitySummaryKey = "Summary";

		#endregion

		#region Translation Keys

		// Define all of the keys to be used for translations, these match the keys in the translation files
		private const string LockStateTranslationKey = "LockStateLabel";
		private const string BatteryPercentTranslationKey = "BatteryPercentLabel";
		private const string MasterPinTranslationKey = "MasterPinLabel";
		private const string AutoLockTranslationKey = "AutoLockLabel";
		private const string AutoLockTimeTranslationKey = "AutoLockTimeLabel";

		private const string UserListTranslationKey = "UserListLabel";
		private const string UserNameTranslationKey = "UserNameLabel";
		private const string UserIdTranslationKey = "UserIdLabel";
		private const string UserPinTranslationKey = "UserPinLabel";
		private const string UserEnabledTranslationKey = "UserEnabledLabel";
		private const string UserPermissionTranslationKey = "PermissionLevelLabel";
		private const string AdminTranslationKey = "AdminLabel";
		private const string ResidentTranslationKey = "ResidentLabel";
		private const string GuestTranslationKey = "GuestLabel";

		#endregion

		#region Programming

		private const string LockLabel = "^LockLabel";
		private const string LockWithDelayLabel = "^LockWithDelayLabel";
		private const string DelayLabel = "^DelayLabel";
		private const string UnlockLabel = "^UnlockLabel";
		private const string UnlockWithAutoRelockLabel = "^UnlockWithAutoRelockLabel";
		private const string AutoRelockTimeLabel = "^AutoLockTimeLabel";
		private const string EnableAutoLockLabel = "^EnableAutoLockLabel";
		private const string DisableAutoLockLabel = "^DisableAutoLockLabel";
		private const string SetAutoLockTimeLabel = "^SetAutoLockTimeLabel";

		private const string AppUserName = "App";
		private const string SequenceUserName = "Sequence";

		#endregion

		#endregion

		#region Fields

		private DoorLockEmulator _doorLockEmulator;

		private ClassDefinition _userClass;
		private ClassDefinition _lockActivityClass;

		/// <summary>
		/// Maps the user id received from the door lock to the user id of the user instance in the driver
		/// </summary>
		private readonly Dictionary<int, ObjectValue> _lockUserIdToDriverUser = new Dictionary<int, ObjectValue>();

		/// <summary>
		/// Maps the id of the user instance to the user id received from the door lock
		/// </summary>
		private readonly Dictionary<string, DoorLockUser> _driverUserIdToLockUser = new Dictionary<string, DoorLockUser>();

		private ObjectList _userList;
		private ObjectList _activityFeed;
		private PropertyValue<string> _lockStateProperty;
		private PropertyValue<string> _lockStateIconProperty;
		private PropertyValue<int> _batteryPercentProperty;
		private PropertyValue<string> _batteryIconProperty;
		private PropertyValue<string> _masterPinProperty;
		private PropertyValue<bool> _autoLockProperty;
		private PropertyValue<int> _autoLockTimeProperty;

		#endregion

		#region Constructor

		public DoorLock()
		{
			AddUserAttributes();
			CreateDeviceDefinition();
		}

		#endregion

		#region AExtensionDevice Members

		protected override IOperationResult SetDriverPropertyValue<T>(string propertyKey, T value)
		{
			switch (propertyKey)
			{
				case AutoLockKey:
					var autoLock = value as bool?;
					if (autoLock == null)
						return new OperationResult(OperationResultCode.Error, "The value provided could not be converted to a bool.");

					_doorLockEmulator.AutoLock = (bool)autoLock;
					return new OperationResult(OperationResultCode.Success);

				case AutoLockTimeKey:
					var autoLockTime = value as int?;
					if (autoLockTime == null)
						return new OperationResult(OperationResultCode.Error, "The value provided could not be converted to an int.");

					_doorLockEmulator.AutoLockTime = (int)autoLockTime;
					return new OperationResult(OperationResultCode.Success);
			}

			return new OperationResult(OperationResultCode.Error, "The property does not exist.");
		}

		protected override IOperationResult SetDriverPropertyValue<T>(string objectId, string propertyKey, T value)
		{
			DoorLockUser user;
			if (!_driverUserIdToLockUser.TryGetValue(objectId, out user))
				return new OperationResult(OperationResultCode.Error, "The user you are trying to modify does not exist.");

			switch (propertyKey)
			{
				case UserNameKey:
					var name = value as string;
					_doorLockEmulator.ModifyUser(user.Id, name, user.Pin, user.Enabled, user.AccessLevel);
					return new OperationResult(OperationResultCode.Success);

				case UserPinKey:
					var pin = value as string;
					_doorLockEmulator.ModifyUser(user.Id, user.Name, pin, user.Enabled, user.AccessLevel);
					return new OperationResult(OperationResultCode.Success);

				case UserEnabledKey:
					var enabled = value as bool?;
					if (enabled == null)
						return new OperationResult(OperationResultCode.Error, "The value provided could not be converted to a bool.");

					_doorLockEmulator.ModifyUser(user.Id, user.Name, user.Pin, (bool)enabled, user.AccessLevel);
					return new OperationResult(OperationResultCode.Success);

				case UserPermissionKey:
					var permissionString = value as string;
					PermissionLevel permission;
					switch (permissionString)
					{
						case AdminLabel:
							permission = PermissionLevel.Admin;
							break;
						case GuestLabel:
							permission = PermissionLevel.Guest;
							break;
						case ResidentLabel:
							permission = PermissionLevel.Resident;
							break;
						default:
							return new OperationResult(OperationResultCode.Error, "This permission level does not exist.");
					}

					_doorLockEmulator.ModifyUser(user.Id, user.Name, user.Pin, user.Enabled, permission);
					return new OperationResult(OperationResultCode.Success);
			}

			return new OperationResult(OperationResultCode.Error, "The property does not exist.");
		}

		protected override IOperationResult DoCommand(string command, string[] parameters)
		{
			// ReSharper disable once ObjectCreationAsStatement
			new Thread(DoCommandThreadCallback, new DoCommandObject(command, parameters));
			return new OperationResult(OperationResultCode.Success);
		}

		#endregion

		#region ICloudConnected Members

		public void Initialize()
		{
			// Generally in this method you would initialize the connection to the cloud service that talks to the device,
			// but in this case since this is a fake driver we will simply initialize the class that simulates the door lock
			_doorLockEmulator = new DoorLockEmulator();
			_doorLockEmulator.StateChangedEvent += DoorLockEmulatorOnStateChangedEvent;

			ConnectionTransport = new DoorLockTransport();
			DeviceProtocol = new DoorLockProtocol(ConnectionTransport, Id);
			DeviceProtocol.Initialize(DriverData);

			AddExampleUsersToList();
		}

		#endregion

		#region IConnection Members

		public override void Connect()
		{
			Connected = true;

			// Generally in this method you would connect to the actual device and update the UI with data from the device,
			// but in this case since this is a fake driver we will simply refresh the data from the door lock simulator
			Refresh();
		}

		#endregion

		#region Programmable Operations

		[ProgrammableOperation(LockLabel)]
		public void Lock()
		{
			if (EnableLogging)
				Log("Lock command triggered from sequence");

			_doorLockEmulator.Lock(SequenceUserName);
		}

		[ProgrammableOperation(LockWithDelayLabel)]
		public void Lock(
			[Display(DelayLabel)]
			[Min(0)]
			[Max(600)]
			[Unit(Unit.Seconds)]
			int delayTime)
		{
			if (EnableLogging)
				Log(string.Format("Lock command with {0} second delay triggered from sequence", delayTime));

			_doorLockEmulator.Lock(SequenceUserName, delayTime);
		}

		[ProgrammableOperation(UnlockLabel)]
		public void Unlock()
		{
			if (EnableLogging)
				Log("Unlock command triggered from sequence");

			_doorLockEmulator.Unlock(SequenceUserName);
		}

		[ProgrammableOperation(UnlockWithAutoRelockLabel)]
		public void Unlock(
			[Display(AutoRelockTimeLabel)]
			[Min(0)]
			[Max(600)]
			[Unit(Unit.Seconds)]
			int autoLockTime)
		{
			if (EnableLogging)
				Log(string.Format("Unlock command with {0} second auto re-lock triggered from sequence", autoLockTime));

			_doorLockEmulator.Unlock(SequenceUserName, true, autoLockTime);
		}

		[ProgrammableOperation(EnableAutoLockLabel)]
		public void EnableAutoLock()
		{
			if (EnableLogging)
				Log("EnableAutoLock command triggered from sequence");

			_doorLockEmulator.AutoLock = true;
		}

		[ProgrammableOperation(DisableAutoLockLabel)]
		public void DisableAutoLock()
		{
			if (EnableLogging)
				Log("DisableAutoLock command triggered from sequence");

			_doorLockEmulator.AutoLock = false;
		}

		[ProgrammableOperation(SetAutoLockTimeLabel)]
		public void SetAutoLockTime(
			[Display(AutoRelockTimeLabel)]
			[Min(0)]
			[Max(600)]
			[Unit(Unit.Seconds)]
			int autoLockTime)
		{
			if (EnableLogging)
				Log(string.Format("SetAutoLockTime command with {0} second auto lock triggered from sequence", autoLockTime));

			_doorLockEmulator.AutoLockTime = autoLockTime;
		}

		#endregion

		#region Programmable Events

		[ProgrammableEvent(LockedLabel)]
		[TriggeredBy(LockLabel, LockWithDelayLabel)]
		public event EventHandler Locked;

		[ProgrammableEvent(UnlockedLabel)]
		[TriggeredBy(UnlockLabel)]
		public event EventHandler Unlocked;

		[ProgrammableEvent(JammedLabel)]
		public event EventHandler Jammed;

		[ProgrammableEvent(BatteryLowLabel)]
		public event EventHandler BatteryLow;

		#endregion

		#region Private Methods

		private void CreateDeviceDefinition()
		{
			// Define the user object
			_userClass = CreateClassDefinition(UserKey);
			_userClass.AddProperty(new PropertyDefinition(UserNameKey, UserNameTranslationKey, DevicePropertyType.String));
			_userClass.AddProperty(new PropertyDefinition(UserIdKey, UserIdTranslationKey, DevicePropertyType.Int32));
			_userClass.AddProperty(new PropertyDefinition(UserPinKey, UserPinTranslationKey, DevicePropertyType.String, 4, 8, 1));
			_userClass.AddProperty(new PropertyDefinition(UserEnabledKey, UserEnabledTranslationKey, DevicePropertyType.Boolean));

			var permisionLevelValues = new List<IPropertyAvailableValue>
			{
				new PropertyAvailableValue<string>(GuestLabel, DevicePropertyType.String, GuestTranslationKey, null),
				new PropertyAvailableValue<string>(ResidentLabel, DevicePropertyType.String, ResidentTranslationKey, null),
				new PropertyAvailableValue<string>(AdminLabel, DevicePropertyType.String, AdminTranslationKey, null)
			};
			_userClass.AddProperty(new PropertyDefinition(UserPermissionKey, UserPermissionTranslationKey, DevicePropertyType.String, permisionLevelValues));

			// Define the user list property
			_userList = CreateList(new PropertyDefinition(UserListKey, UserListTranslationKey, DevicePropertyType.ObjectList, _userClass));

			// Define the lock activity object
			_lockActivityClass = CreateClassDefinition(LockActivityKey);
			_lockActivityClass.AddProperty(new PropertyDefinition(ActivityUserNameKey, string.Empty, DevicePropertyType.String));
			_lockActivityClass.AddProperty(new PropertyDefinition(ActivityEventTypeKey, string.Empty, DevicePropertyType.String));
			_lockActivityClass.AddProperty(new PropertyDefinition(ActivitySuccessKey, string.Empty, DevicePropertyType.Boolean));
			_lockActivityClass.AddProperty(new PropertyDefinition(ActivityTimeKey, string.Empty, DevicePropertyType.String));
			_lockActivityClass.AddProperty(new PropertyDefinition(ActivitySummaryKey, string.Empty, DevicePropertyType.String));

			// Define the activity feed property
			_activityFeed = CreateList(
				new PropertyDefinition(LockActivityFeedKey, string.Empty, DevicePropertyType.ObjectList, _lockActivityClass));

			// Define the battery percent property
			_batteryPercentProperty = CreateProperty<int>(
				new PropertyDefinition(BatteryPercentKey, BatteryPercentTranslationKey, DevicePropertyType.Int32, 0, 100, 1));

			// Define the battery icon property
			_batteryIconProperty = CreateProperty<string>(new PropertyDefinition(BatteryIconKey, null, DevicePropertyType.String));

			// Define the lock state property
			_lockStateProperty = CreateProperty<string>(
				new PropertyDefinition(LockStateKey, LockStateTranslationKey, DevicePropertyType.String));

			// Define the lock state icon property
			_lockStateIconProperty = CreateProperty<string>(new PropertyDefinition(LockStateIconKey, null, DevicePropertyType.String));

			// Define the master pin property
			_masterPinProperty = CreateProperty<string>(
				new PropertyDefinition(MasterPinKey, MasterPinTranslationKey, DevicePropertyType.String, 4, 8, 1));

			// Define the auto lock property
			_autoLockProperty = CreateProperty<bool>(new PropertyDefinition(AutoLockKey, AutoLockTranslationKey, DevicePropertyType.Boolean));

			// Define the auto lock time property
			_autoLockTimeProperty = CreateProperty<int>(
				new PropertyDefinition(AutoLockTimeKey, AutoLockTimeTranslationKey, DevicePropertyType.Int32, 10, 600, 1));
		}

		/// <summary>
		/// Update the state of all properties.
		/// </summary>
		private void Refresh()
		{
			if (_doorLockEmulator == null)
				return;

			// Refresh lock state
			SetLockState(_doorLockEmulator.LockState);

			// Refresh battery status
			_batteryPercentProperty.Value = _doorLockEmulator.BatteryPercent;
			SetBatteryState(_doorLockEmulator.BatteryState);

			// Refresh settings
			_masterPinProperty.Value = _doorLockEmulator.MasterPin;
			_autoLockProperty.Value = _doorLockEmulator.AutoLock;
			_autoLockTimeProperty.Value = _doorLockEmulator.AutoLockTime;

			// Refresh user list
			ClearUsers();
			foreach (var user in _doorLockEmulator.Users)
			{
				AddUser(user);
			}

			// Refresh activity feed
			_activityFeed.Clear();
			foreach (var activity in _doorLockEmulator.ActivityFeed)
			{
				AddLockActivity(activity);
			}

			Commit();
		}

		private object DoCommandThreadCallback(object userSpecific)
		{
			var doCommandObject = (DoCommandObject)userSpecific;
			var command = doCommandObject.Command;
			var parameters = doCommandObject.Parameters;

			switch (command)
			{
				case LockCommand:
					_doorLockEmulator.Lock(AppUserName);
					break;

				case UnlockCommand:
					_doorLockEmulator.Unlock(AppUserName);
					break;

				case ToggleCommand:
					// If the lock is in a transitioning state (ie. locking) do nothing
					switch (_doorLockEmulator.LockState)
					{
						case LockState.Unlocked:
							_doorLockEmulator.Lock(AppUserName);
							break;
						case LockState.Locked:
						case LockState.Jammed:
							_doorLockEmulator.Unlock(AppUserName);
							break;
					}
					break;

				case ChangeMasterPinCommand:
					_doorLockEmulator.ChangeMasterPin(parameters[0]);
					break;

				case AddUserCommand:
					_doorLockEmulator.AddUser(parameters[0], parameters[1], true, PermissionLevel.Guest);
					break;

				case DeleteUserCommand:
					_doorLockEmulator.RemoveUser(int.Parse(parameters[0]));
					break;
			}

			return null;
		}

		private void AddUser(DoorLockUser user)
		{
			// Create the user instance
			var userInstance = CreateObject(_userClass);
			userInstance.GetValue<string>(UserNameKey).Value = user.Name;
			userInstance.GetValue<int>(UserIdKey).Value = user.Id;
			userInstance.GetValue<string>(UserPinKey).Value = user.Pin;
			userInstance.GetValue<bool>(UserEnabledKey).Value = user.Enabled;

			switch (user.AccessLevel)
			{
				case PermissionLevel.Admin:
					userInstance.GetValue<string>(UserPermissionKey).Value = AdminLabel;
					break;
				case PermissionLevel.Resident:
					userInstance.GetValue<string>(UserPermissionKey).Value = ResidentLabel;
					break;
				case PermissionLevel.Guest:
					userInstance.GetValue<string>(UserPermissionKey).Value = GuestLabel;
					break;
			}

			// Add the user to the list of users
			_userList.AddObject(userInstance);

			_lockUserIdToDriverUser.Add(user.Id, userInstance);
			_driverUserIdToLockUser.Add(userInstance.Id, user);
		}

		private void RemoveUser(int userId)
		{
			if (!_lockUserIdToDriverUser.ContainsKey(userId))
				return;

			var user = _lockUserIdToDriverUser[userId];

			_userList.RemoveObject(user.Id);
			DeleteProperty(user);

			_lockUserIdToDriverUser.Remove(userId);
			_driverUserIdToLockUser.Remove(user.Id);
		}

		private static void UpdateUser(ObjectValue userInstance, DoorLockUser user)
		{
			userInstance.GetValue<string>(UserNameKey).Value = user.Name;
			userInstance.GetValue<int>(UserIdKey).Value = user.Id;
			userInstance.GetValue<string>(UserPinKey).Value = user.Pin;
			userInstance.GetValue<bool>(UserEnabledKey).Value = user.Enabled;

			switch (user.AccessLevel)
			{
				case PermissionLevel.Admin:
					userInstance.GetValue<string>(UserPermissionKey).Value = AdminLabel;
					break;
				case PermissionLevel.Resident:
					userInstance.GetValue<string>(UserPermissionKey).Value = ResidentLabel;
					break;
				case PermissionLevel.Guest:
					userInstance.GetValue<string>(UserPermissionKey).Value = GuestLabel;
					break;
			}
		}

		private void ClearUsers()
		{
			_userList.Clear();
			_lockUserIdToDriverUser.Clear();
			_driverUserIdToLockUser.Clear();
		}

		private void AddLockActivity(DoorLockEvent lockEvent)
		{
			// Create the activity instance
			var activityInstance = CreateObject(_lockActivityClass);
			activityInstance.GetValue<string>(ActivityUserNameKey).Value = lockEvent.UserName;
			activityInstance.GetValue<string>(ActivityEventTypeKey).Value = lockEvent.EventType.ToString();
			activityInstance.GetValue<bool>(ActivitySuccessKey).Value = lockEvent.Success;
			activityInstance.GetValue<string>(ActivityTimeKey).Value = lockEvent.Time.ToShortTimeString();
			activityInstance.GetValue<string>(ActivitySummaryKey).Value = lockEvent.Summary;

			// Add the activity to the activity feed
			_activityFeed.AddObject(activityInstance);
		}

		private void SetLockState(LockState state)
		{
			switch (state)
			{
				case LockState.Locked:
					_lockStateProperty.Value = LockedLabel;
					_lockStateIconProperty.Value = LockedIcon;
					RaiseLockedEvent();
					break;
				case LockState.Locking:
					_lockStateProperty.Value = LockingLabel;
					_lockStateIconProperty.Value = SpinnerIcon;
					break;
				case LockState.Unlocked:
					_lockStateProperty.Value = UnlockedLabel;
					_lockStateIconProperty.Value = UnlockedIcon;
					RaiseUnlockedEvent();
					break;
				case LockState.Unlocking:
					_lockStateProperty.Value = UnlockingLabel;
					_lockStateIconProperty.Value = SpinnerIcon;
					break;
				case LockState.Jammed:
					_lockStateProperty.Value = JammedLabel;
					_lockStateIconProperty.Value = JammedIcon;
					RaiseJammedEvent();
					break;
			}
		}

		private void SetBatteryState(BatteryLevel level)
		{
			switch (level)
			{
				case BatteryLevel.Critical:
				case BatteryLevel.Low:
					_batteryIconProperty.Value = BatteryLowIcon;
					RaiseBatteryLowEvent();
					break;
				case BatteryLevel.Normal:
					_batteryIconProperty.Value = BatteryNormalIcon;
					break;
			}
		}

		private void AddUserAttributes()
		{
			AddUserAttribute(
				UserAttributeType.MessageBox,
				"messageBox1",
				"Example Lable",
				"This is an example description",
				false,
				UserAttributeRequiredForConnectionType.After);

			AddUserAttribute(
				UserAttributeType.Custom,
				"macAddress",
				"MAC Address",
				"This is an example description",
				true,
				UserAttributeRequiredForConnectionType.Before,
				UserAttributeDataType.String,
				string.Empty);
		}

		private void AddExampleUsersToList()
		{
			_doorLockEmulator.AddUser("James Smith", "1234", true, PermissionLevel.Admin);
			_doorLockEmulator.AddUser("Dana Jones", "4321", true, PermissionLevel.Admin);
		}

		private void RaiseLockedEvent()
		{
			var locked = Locked;

			if (locked != null)
				locked.Invoke(this, new EventArgs());
		}

		private void RaiseUnlockedEvent()
		{
			var unlocked = Unlocked;

			if (unlocked != null)
				unlocked.Invoke(this, new EventArgs());
		}

		private void RaiseJammedEvent()
		{
			var jammed = Jammed;

			if (jammed != null)
				jammed.Invoke(this, new EventArgs());
		}

		private void RaiseBatteryLowEvent()
		{
			var batteryLow = BatteryLow;

			if (batteryLow != null)
				batteryLow.Invoke(this, new EventArgs());
		}

		#endregion

		#region Event Handlers

		private void DoorLockEmulatorOnStateChangedEvent(object sender, StateChangeEventArgs stateChangeEventArgs)
		{
			switch (stateChangeEventArgs.EventType)
			{
				case EventType.LockStateChanged:
					SetLockState((LockState)stateChangeEventArgs.EventData);
					break;

				case EventType.MasterPinChanged:
					_masterPinProperty.Value = (string)stateChangeEventArgs.EventData;
					break;

				case EventType.UserAdded:
					AddUser((DoorLockUser)stateChangeEventArgs.EventData);
					break;

				case EventType.UserRemoved:
					RemoveUser((int)stateChangeEventArgs.EventData);
					break;

				case EventType.UserChanged:
					var userChangedData = (DoorLockUser)stateChangeEventArgs.EventData;

					ObjectValue userObject;
					if (_lockUserIdToDriverUser.TryGetValue(userChangedData.Id, out userObject))
						UpdateUser(userObject, userChangedData);
					break;

				case EventType.BatteryLevelChanged:
					SetBatteryState((BatteryLevel)stateChangeEventArgs.EventData);
					break;

				case EventType.BatteryPercentChanged:
					_batteryPercentProperty.Value = (int)stateChangeEventArgs.EventData;
					break;

				case EventType.AutoLockChanged:
					_autoLockProperty.Value = (bool)stateChangeEventArgs.EventData;
					break;

				case EventType.AutoLockTimeChanged:
					_autoLockTimeProperty.Value = (int)stateChangeEventArgs.EventData;
					break;

				case EventType.DoorLockEvent:
					AddLockActivity((DoorLockEvent)stateChangeEventArgs.EventData);
					break;
			}

			Commit();
		}

		#endregion
	}

	internal class DoCommandObject
	{
		public DoCommandObject(string command, string[] parameters)
		{
			Command = command;
			Parameters = parameters;
		}

		public string Command;
		public string[] Parameters;
	}
}