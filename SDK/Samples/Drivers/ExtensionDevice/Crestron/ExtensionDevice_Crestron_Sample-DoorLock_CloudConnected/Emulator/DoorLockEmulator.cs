using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;

namespace ExtensionDevice_Crestron_Sample_DoorLock_CloudConnected.Emulator
{
	public class DoorLockEmulator
	{
		private const int EventDelay = 2000; // 2 seconds
		private const int BatteryLevelDelay = 60000; // 1 minute
		private readonly CTimer _autoLockTimer;
		private readonly CTimer _lockingTimer;
		private readonly CTimer _unlockingTimer;

		private UnlockingCallbackObject _unlockingInfo = new UnlockingCallbackObject();
		private string _lockingInfo = string.Empty;

		private LockState _lockState = LockState.Locked;
		private BatteryLevel _batteryState = BatteryLevel.Normal;
		private int _batteryPercent = 100;
		private string _masterPin = "1234";
		private bool _autoLock = true;
		private int _autoLockTime = 30;

		public DoorLockEmulator()
		{
			Users = new List<DoorLockUser>();
			ActivityFeed = new List<DoorLockEvent>();

			// ReSharper disable once ObjectCreationAsStatement
			new CTimer(BatteryCallbackFunction, null, BatteryLevelDelay, BatteryLevelDelay);
			_autoLockTimer = new CTimer(AutoLockCallbackFunction, Timeout.Infinite);
			_lockingTimer = new CTimer(LockingCallbackFunction, Timeout.Infinite);
			_unlockingTimer = new CTimer(UnlockingCallbackFunction, Timeout.Infinite);
		}

		public List<DoorLockUser> Users { get; private set; }

		public List<DoorLockEvent> ActivityFeed { get; private set; } 

		public LockState LockState
		{
			get { return _lockState; }
			private set
			{
				if (_lockState == value)
					return;

				_lockState = value;
				StateChangedEvent.Invoke(this, new StateChangeEventArgs(EventType.LockStateChanged, _lockState));
			}
		}

		public BatteryLevel BatteryState
		{
			get { return _batteryState; }
			private set
			{
				if (_batteryState == value)
					return;

				_batteryState = value;
				StateChangedEvent.Invoke(this, new StateChangeEventArgs(EventType.BatteryLevelChanged, _batteryState));
			}
		}

		public int BatteryPercent
		{
			get { return _batteryPercent; }
			private set
			{
				if (_batteryPercent == value)
					return;

				_batteryPercent = value;
				StateChangedEvent.Invoke(this, new StateChangeEventArgs(EventType.BatteryPercentChanged, _batteryPercent));
			}
		}

		public string MasterPin
		{
			get { return _masterPin; }
			private set
			{
				if (_masterPin == value)
					return;

				_masterPin = value;
				StateChangedEvent.Invoke(this, new StateChangeEventArgs(EventType.MasterPinChanged, _masterPin));
			}
		}

		public bool AutoLock
		{
			get { return _autoLock; }
			set
			{
				if (_autoLock == value)
					return;

				_autoLock = value;
				StateChangedEvent.Invoke(this, new StateChangeEventArgs(EventType.AutoLockChanged, _autoLock));
			}
		}

		public int AutoLockTime
		{
			get { return _autoLockTime; }
			set
			{
				if (_autoLockTime == value)
					return;

				_autoLockTime = value;
				StateChangedEvent.Invoke(this, new StateChangeEventArgs(EventType.AutoLockTimeChanged, _autoLockTime));
			}
		}

		public void Lock(string userName)
		{
			if (_lockState == LockState.Locked || _lockState == LockState.Locking || _lockState == LockState.Jammed)
				return;

			// If manually locked, cancel the auto lock timer
			_autoLockTimer.Stop();

			// Set state to locking and reset the locking timer
			LockState = LockState.Locking;
			_lockingInfo = userName;
			_lockingTimer.Reset(EventDelay);
		}

		public void Lock(string userName, int delayTime)
		{
			if (_lockState == LockState.Locked || _lockState == LockState.Locking || _lockState == LockState.Jammed)
				return;

			_autoLockTimer.Reset(delayTime * 1000);
		}

		public void Unlock(string userName)
		{
			Unlock(userName, AutoLock, AutoLockTime);
		}

		public void Unlock(string userName, bool autoLock, int autoLockTime)
		{
			if (_lockState == LockState.Unlocked || _lockState == LockState.Unlocking)
				return;

			// Set the state to unlocking and reset the unlocking timer
			LockState = LockState.Unlocking;
			_unlockingInfo = new UnlockingCallbackObject { UserName = userName, AutoLock = autoLock, AutoLockTime = autoLockTime };
			_unlockingTimer.Reset(EventDelay);
		}

		public void ChangeMasterPin(string newPin)
		{
			MasterPin = newPin;
		}

		public void AddUser(string name, string pin, bool enabled, PermissionLevel accessLevel)
		{
			var user = new DoorLockUser(name, pin, enabled, accessLevel);
			Users.Add(user);
			StateChangedEvent.Invoke(this, new StateChangeEventArgs(EventType.UserAdded, user));
		}

		public void RemoveUser(int userId)
		{
			Users.RemoveAll(x => x.Id == userId);
			StateChangedEvent.Invoke(this, new StateChangeEventArgs(EventType.UserRemoved, userId));
		}

		public void ModifyUser(int userId, string name, string pin, bool enabled, PermissionLevel accessLevel)
		{
			var user = Users.FirstOrDefault(x => x.Id == userId);
			if (user == null)
				return;

			user.Name = name;
			user.Pin = pin;
			user.Enabled = enabled;
			user.AccessLevel = accessLevel;

			StateChangedEvent.Invoke(this, new StateChangeEventArgs(EventType.UserChanged, user));
		}

		private void BatteryCallbackFunction(object notUsed)
		{
			BatteryPercent--;

			if (BatteryPercent <= 0)
			{
				BatteryPercent = 100;
				BatteryState = BatteryLevel.Normal;
				return;
			}

			if (BatteryPercent <= 5)
				BatteryState = BatteryLevel.Critical;
			else if (BatteryPercent <= 20)
				BatteryState = BatteryLevel.Low;
			else
				BatteryState = BatteryLevel.Normal;
		}

		private void AutoLockCallbackFunction(object notUsed)
		{
			Lock("Auto Lock");
		}

		private void LockingCallbackFunction(object notUsed)
		{
			// 10% chance that the lock jams
			var random = new Random();
			LockState = random.Next(10) < 1 ? LockState.Jammed : LockState.Locked;
			CreateNewLockEvent(_lockingInfo, LockState.Locked, true, DateTime.Now);
		}

		private void UnlockingCallbackFunction(object notUsed)
		{
			LockState = LockState.Unlocked;
			CreateNewLockEvent(_unlockingInfo.UserName, LockState.Unlocked, true, DateTime.Now);

			// If autolock is enabled, reset the timer
			if (_unlockingInfo.AutoLock)
				_autoLockTimer.Reset(_unlockingInfo.AutoLockTime * 1000);
		}

		private void CreateNewLockEvent(string userName, LockState eventType, bool success, DateTime time)
		{
			var lockEvent = new DoorLockEvent(userName, eventType, success, time);
			ActivityFeed.Add(lockEvent);

			StateChangedEvent.Invoke(this, new StateChangeEventArgs(EventType.DoorLockEvent, lockEvent));
		}

		public event EventHandler<StateChangeEventArgs> StateChangedEvent;
	}

	public class DoorLockUser
	{
		private static int _startingUserId = 1;

		public DoorLockUser(string name, string pin, bool enabled, PermissionLevel accessLevel)
		{
			Name = name;
			Pin = pin;
			Enabled = enabled;
			AccessLevel = accessLevel;
			Id = _startingUserId;

			_startingUserId++;
		}

		public int Id { get; private set; }
		public string Name { get; set; }
		public string Pin { get; set; }
		public bool Enabled { get; set; }
		public PermissionLevel AccessLevel { get; set; }
	}

	public class DoorLockEvent
	{
		public DoorLockEvent(string userName, LockState eventType, bool success, DateTime time)
		{
			UserName = userName;
			EventType = eventType;
			Success = success;
			Time = time;
			Summary = string.Format("{0} by {1} at {2}", eventType, userName, time.ToShortTimeString());
		}

		public string UserName { get; set; }
		public LockState EventType { get; set; }
		public bool Success { get; set; }
		public DateTime Time { get; set; }
		public string Summary { get; set; }
	}

	public class StateChangeEventArgs : EventArgs
	{
		public StateChangeEventArgs(EventType eventType, object eventData)
		{
			EventType = eventType;
			EventData = eventData;
		}

		public EventType EventType { get; private set; }

		public object EventData { get; private set; }
	}

	public class UnlockingCallbackObject
	{
		public string UserName { get; set; }

		public bool AutoLock { get; set; }

		public int AutoLockTime { get; set; }
	}

	public enum LockState
	{
		Locked,
		Locking,
		Unlocked,
		Unlocking,
		Jammed
	}

	public enum PermissionLevel
	{
		Admin,
		Resident,
		Guest
	}

	public enum BatteryLevel
	{
		Critical,
		Low,
		Normal
	}

	public enum EventType
	{
		LockStateChanged,
		MasterPinChanged,
		UserAdded,
		UserRemoved,
		UserChanged,
		BatteryLevelChanged,
		BatteryPercentChanged,
		AutoLockChanged,
		AutoLockTimeChanged,
		DoorLockEvent
	}
}