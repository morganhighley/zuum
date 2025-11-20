using System;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces;
using Crestron.SimplSharp;
using Crestron.RAD.DeviceTypes.SecuritySystem;
using System.Collections.Generic;
using Crestron.RAD.Common.BasicDriver;

namespace SecuritySystem_Crestron_SampleDriverModel_Serial
{
	/// <summary>
	/// This class is used to define the security system keypad
	/// </summary>
	public class SecuritySystemKeypad : IEmulatedSecuritySystemKeypad, IDisposable
	{
		#region Fields

		protected SecuritySystemProtocol SecuritySystemProtocol;
		protected CTimer ArrowKeyRampTimer;
		protected ArrowDirections ArrowKeyRampingDirection;
		protected bool ArrowKeyIsRamping;
		private int _rampingTickRate = 500;

		#endregion

		#region Ctor

		/// <summary>
		/// Default constructor
		/// </summary>
		public SecuritySystemKeypad()
		{

		}

		#endregion

		#region Property

		protected bool Initialized
		{
			get { return SecuritySystemProtocol != null; }
		}

		#endregion

		#region Public/protected Method

		/// <summary>
		/// Initialize the security system protocol
		/// </summary>
		/// <param name="protocol">Sample Security System</param>
		public void Initialize(SecuritySystemProtocol protocol)
		{
			if (SecuritySystemProtocol != null)
				SecuritySystemProtocol.StateChange -= SecuritySystemProtocolOnStateChange;
			SecuritySystemProtocol = protocol;
			SecuritySystemProtocol.StateChange += SecuritySystemProtocolOnStateChange;
		}

		/// <summary>
		/// Dipose the object
		/// </summary>
		public void Dispose()
		{
			//todo
			if (ArrowKeyRampTimer != null)
			{
				ArrowKeyRampTimer.Stop();
				ArrowKeyRampTimer.Dispose();
				ArrowKeyRampTimer = null;
				SecuritySystemProtocol.StateChange -= SecuritySystemProtocolOnStateChange;
			}
		}

		protected void PrepareStringThenSend(CommandSet commandSet)
		{
			SecuritySystemProtocol.SendCommandInternal(commandSet);
		}

		#endregion

		#region Numeric Keypad

		/// <summary>
		/// Property indicating that the KeypadNumber command is supported.
		/// </summary>
		public bool SupportsKeypadNumber { get { return false; } }

		/// <summary>
		/// Sends a keypad number to the device.
		/// </summary>
		/// <param name="number">Number to be sent to the device.</param>
		public void KeypadNumber(uint num)
		{
			if (!Initialized)
			{
				return;
			}

			if (!SupportsKeypadNumber)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem, does not support KeypadNumber.");
				return;
			}

			if (num == 0 && SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum._0))
			{
				var command = new CommandSet("0", "0", CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, StandardCommandsEnum._0);
				PrepareStringThenSend(command);
			}

			while (num > 0)
			{
				var digit = num % 10;
				StandardCommandsEnum numberEnum;
				switch (digit)
				{
					case 0:
						numberEnum = StandardCommandsEnum._0;
						break;
					case 1:
						numberEnum = StandardCommandsEnum._1;
						break;
					case 2:
						numberEnum = StandardCommandsEnum._2;
						break;
					case 3:
						numberEnum = StandardCommandsEnum._3;
						break;
					case 4:
						numberEnum = StandardCommandsEnum._4;
						break;
					case 5:
						numberEnum = StandardCommandsEnum._5;
						break;
					case 6:
						numberEnum = StandardCommandsEnum._6;
						break;
					case 7:
						numberEnum = StandardCommandsEnum._7;
						break;
					case 8:
						numberEnum = StandardCommandsEnum._8;
						break;
					case 9:
						numberEnum = StandardCommandsEnum._9;
						break;
					default:
						return;
				}
				if (!SecuritySystemProtocol.CommandsDictionary.ContainsKey(numberEnum)) return;

				var command = new CommandSet(numberEnum.ToString(), numberEnum.ToString(), CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, numberEnum);
				PrepareStringThenSend(command);

				num = num / 10;
			}
		}

		/// <summary>
		/// Property indicating that the Keypad Pound command is supported.
		/// </summary>
		public bool SupportsPound { get { return false; } }

		/// <summary>
		/// Method to send a Keypad "#" to the Device.
		/// </summary>
		public void Pound()
		{
			if (!Initialized)
			{
				return;
			}

			if (!SupportsPound)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem, does not support Keypad Pound.");
				return;
			}
			if (SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum.Octothorpe))
			{
				var command = new CommandSet("KeypadPound", StandardCommandsEnum.Octothorpe.ToString(), CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, StandardCommandsEnum.KeypadPound);
				PrepareStringThenSend(command);
			}
		}

		/// <summary>
		/// Property indicating that the Keypad Asterisk command is supported.
		/// </summary>
		public bool SupportsAsterisk
		{
			get { return false; }
		}

		/// <summary>
		/// Method to send a Keypad "*" to the Device.
		/// </summary>
		public void Asterisk()
		{
			if (!Initialized)
			{
				return;
			}

			if (!SupportsAsterisk)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem, does not support Keypad Asterisk.");
				return;
			}
			if (SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum.Asterisk))
			{
				var command = new CommandSet("KeypadAsterisk", StandardCommandsEnum.Asterisk.ToString(), CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, StandardCommandsEnum.Asterisk);
				PrepareStringThenSend(command);
			}
		}

		/// <summary>
		/// Property indicating that the Keypad Period command is supported.
		/// </summary>
		public bool SupportsPeriod { get { return false; } }

		/// <summary>
		/// Method to send a Keypad "." to the Device.
		/// </summary>
		public void Period()
		{
			if (!Initialized)
			{
				return;
			}

			if (!SupportsPeriod)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem, does not support Period.");
				return;
			}
			if (SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum.Period))
			{
				var command = new CommandSet("Period", StandardCommandsEnum.Period.ToString(), CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, StandardCommandsEnum.Period);
				PrepareStringThenSend(command);
			}
		}

		/// <summary>
		/// Property indicating that the Keypad Dash command is supported.
		/// </summary>
		public bool SupportsDash
		{
			get { return false; }
		}

		/// <summary>
		/// Method to send a Keypad "-" to the Device.
		/// </summary>
		public void Dash()
		{
			if (!Initialized)
			{
				return;
			}

			if (!SupportsDash)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem, does not support Dash.");
				return;
			}

			if (SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum.Dash))
			{
				var command = new CommandSet("Dash", StandardCommandsEnum.Dash.ToString(), CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, StandardCommandsEnum.Dash);
				PrepareStringThenSend(command);
			}
		}

		/// <summary>
		/// Method to send a series of keypad characters to the device.
		/// </summary>
		/// <param name="keys"></param>
		public void SendKeypadString(string keys)
		{
			if (!Initialized)
			{
				return;
			}

			if (keys.Length >= 20)
				keys = keys.Substring(0, 20);

			foreach (var key in keys)
			{

				var commandName = string.Empty;
				StandardCommandsEnum commandEnums = StandardCommandsEnum._0;

				switch (key)
				{
					case '0':
						commandName = "0";
						break;
					case '1':
						commandEnums = StandardCommandsEnum._1;
						commandName = "1";
						break;
					case '2':
						commandEnums = StandardCommandsEnum._2;
						commandName = "2";
						break;
					case '3':
						commandEnums = StandardCommandsEnum._3;
						commandName = "3";
						break;
					case '4':
						commandEnums = StandardCommandsEnum._4;
						commandName = "4";
						break;
					case '5':
						commandEnums = StandardCommandsEnum._5;
						commandName = "5";
						break;
					case '6':
						commandEnums = StandardCommandsEnum._6;
						commandName = "6";
						break;
					case '7':
						commandEnums = StandardCommandsEnum._7;
						commandName = "7";
						break;
					case '8':
						commandEnums = StandardCommandsEnum._8;
						commandName = "8";
						break;
					case '9':
						commandEnums = StandardCommandsEnum._9;
						commandName = "9";
						break;
					case '.':
						commandEnums = StandardCommandsEnum.Period;
						commandName = ".";
						break;
					case '-':
						commandEnums = StandardCommandsEnum.Dash;
						commandName = "-";
						break;
					case '*':
						commandEnums = StandardCommandsEnum.Asterisk;
						commandName = "*";
						break;
					case '#':
						commandEnums = StandardCommandsEnum.Octothorpe;
						commandName = "#";
						break;
					default:
						continue;
				}

				var command = new CommandSet(commandName, commandEnums.ToString(),
					CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, commandEnums);

				PrepareStringThenSend(command);
			}
		}

		/// <summary>
		/// Property indicating that the Keypad Back Space command is supported.
		/// </summary>
		public bool SupportsKeypadBackSpace
		{
			get { return false; }
		}

		/// <summary>
		/// Method to send a Back Space to the Device.
		/// </summary>
		public void KeypadBackSpace()
		{
			if (!Initialized)
			{
				return;
			}

			if (!SupportsKeypadBackSpace)
			{
				SecuritySystemProtocol.LogMessage("ASecuritySystem, does not support Keypad Back Space.");
				return;
			}
			if (SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum.KeypadBackSpace))
			{
				var command = new CommandSet("KeypadBackSpace", StandardCommandsEnum.KeypadBackSpace.ToString(), CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, StandardCommandsEnum.KeypadBackSpace);
				PrepareStringThenSend(command);
			}
		}

		/// <summary>
		/// Property defining the "standard" labels for a numeric keypad buttons 0-9
		/// </summary>
		public KeypadLabels[] NumericKeypadLabels
		{
			get
			{
				return new[]
				{
					new KeypadLabels
					{
						PrimaryLabel = "0",
						SecondaryLabel = ""
					},
					new KeypadLabels
					{
						PrimaryLabel = "1",
						SecondaryLabel = ""
					},
					new KeypadLabels
					{
						PrimaryLabel = "2",
						SecondaryLabel = ""
					},
					new KeypadLabels
					{
						PrimaryLabel = "3",
						SecondaryLabel = ""
					},
					new KeypadLabels
					{
						PrimaryLabel = "4",
						SecondaryLabel = ""
					},
					new KeypadLabels
					{
						PrimaryLabel = "5",
						SecondaryLabel = ""
					},
					new KeypadLabels
					{
						PrimaryLabel = "6",
						SecondaryLabel = ""
					},
					new KeypadLabels
					{
						PrimaryLabel = "7",
						SecondaryLabel = ""
					},
					new KeypadLabels
					{
						PrimaryLabel = "8",
						SecondaryLabel = ""
					},
					new KeypadLabels
					{
						PrimaryLabel = "9",
						SecondaryLabel = ""
					}
				};
			}
		}

		/// <summary>
		/// Property defining the "standard" labels for a numeric keypad dash button
		/// </summary>
		public KeypadLabels DashLabels
		{
			get
			{
				return new KeypadLabels
				{
					PrimaryLabel = "-",
					SecondaryLabel = ""
				};
			}
		}

		/// <summary>
		/// Property defining the "standard" labels for a numeric keypad period button
		/// </summary>
		public KeypadLabels PeriodLabels
		{
			get
			{
				return new KeypadLabels
				{
					PrimaryLabel = ".",
					SecondaryLabel = ""
				};
			}
		}

		/// <summary>
		/// Property defining the "standard" labels for a numeric keypad asterisk button
		/// </summary>
		public KeypadLabels AsteriskLabels
		{
			get
			{
				return new KeypadLabels
				{
					PrimaryLabel = "*",
					SecondaryLabel = ""
				};
			}
		}

		/// <summary>
		/// Property defining the "standard" labels for a numeric keypad pound button
		/// </summary>
		public KeypadLabels PoundLabels
		{
			get
			{
				return new KeypadLabels
				{
					PrimaryLabel = "#",
					SecondaryLabel = ""
				};
			}
		}

		#endregion Keypad

		#region Navigation

		/// <summary>
		/// Property indicating that the ArrowKey command is supported.
		/// </summary>
		public bool SupportsArrowKeys
		{
			get { return false; }
		}

		/// <summary>
		/// Property indicating which Arrow Keys are supported.
		/// </summary>
		public List<ArrowDirections> ArrowKeysSupported
		{
			get { return (null); }
		}

		public bool SupportsSelect { get; private set; }

		/// <summary>
		/// Method to send a arrow key to the Device.
		/// </summary>
		/// <param name="direction">Direction of arrow to be send to the device.</param>
		/// <param name="action">Indicates if command should be pressed, held, or released.</param>
		public void ArrowKey(ArrowDirections direction, CommandAction action)
		{
			if (!SupportsArrowKeys)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem : The command ArrowKey is not supported");
				return;
			}

			switch (action)
			{
				case CommandAction.Hold:
					PressArrowKey(direction);
					break;
				case CommandAction.Release:
					ReleaseArrowKey();
					break;
				case CommandAction.None:
					ArrowKey(direction);
					break;
			}
		}

		/// <summary>
		/// Method to send an Arrow Key command to the Device.
		/// </summary>
		/// <param name="direction">Direction of the arrow key</param>
		public void ArrowKey(ArrowDirections direction)
		{
			if (!Initialized)
			{
				return;
			}

			StandardCommandsEnum numberEnum;
			switch (direction)
			{
				case ArrowDirections.Up:
					numberEnum = StandardCommandsEnum.UpArrow;
					break;
				case ArrowDirections.Down:
					numberEnum = StandardCommandsEnum.DownArrow;
					break;
				case ArrowDirections.Left:
					numberEnum = StandardCommandsEnum.LeftArrow;
					break;
				case ArrowDirections.Right:
					numberEnum = StandardCommandsEnum.RightArrow;
					break;
				default:
					return;
			}
			if (!SecuritySystemProtocol.CommandsDictionary.ContainsKey(numberEnum)) return;

			var command = new CommandSet(numberEnum.ToString(), numberEnum.ToString(),
				  CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, numberEnum);

			PrepareStringThenSend(command);
		}

		public void PressArrowKey(ArrowDirections direction)
		{
			if (ArrowKeyRampTimer == null)
			{
				ArrowKeyRampTimer = new CTimer(ArrowKeyTick, null, 0, _rampingTickRate);
			}

			ArrowKeyRampingDirection = direction;
			ArrowKeyIsRamping = true;
		}

		public void ReleaseArrowKey()
		{
			if (ArrowKeyRampTimer == null)
			{
				return;
			}

			ArrowKeyRampTimer.Stop();
			ArrowKeyRampTimer.Dispose();
			ArrowKeyRampTimer = null;

			ArrowKeyIsRamping = false;
		}

		protected void ArrowKeyTick(object obj)
		{
			if (ArrowKeyIsRamping)
			{
				ArrowKey(ArrowKeyRampingDirection);
			}
		}

		public void Select()
		{
			if (!SupportsSelect)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem : The command Select is not supported");
				return;
			}

		}

		/// <summary>
		/// Property indicating that the Enter command is supported.
		/// </summary>
		public bool SupportsEnter
		{
			get { return false; }
		}

		/// <summary>
		/// Method to send the enter command to the Device.
		/// </summary>
		public void Enter()
		{
			if (!Initialized)
			{
				return;
			}

			if (!SupportsEnter)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem : The command Enter is not supported");
				return;
			}

			if (SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum.Enter))
			{
				var command = new CommandSet("Enter", "Enter",
				CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, StandardCommandsEnum.Enter);
				PrepareStringThenSend(command);
			}
		}

		/// <summary>
		/// Property indicating that the Clear command is supported.
		/// </summary>
		public bool SupportsClear
		{
			get { return false; }
		}

		/// <summary>
		/// Method to send the clear command to the device.
		/// </summary>
		public void Clear()
		{
			if (!Initialized)
			{
				return;
			}

			if (!SupportsClear)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem : The command Clear is not supported");
				return;
			}
			if (SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum.Clear))
			{
				var command = new CommandSet("Clear", "Clear",
							  CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, StandardCommandsEnum.Clear);

				PrepareStringThenSend(command);
			}
		}

		/// <summary>
		/// Property indicating that the Exit command is supported.
		/// </summary>
		public bool SupportsExit
		{
			get { return false; }
		}

		/// <summary>
		/// Method to send the exit command to the Device.
		/// </summary>
		public void Exit()
		{
			if (!Initialized)
			{
				return;
			}

			if (!SupportsExit)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem : The command Exit is not supported");
				return;
			}

			if (SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum.Exit))
			{
				var command = new CommandSet("Exit", "Exit",
								CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, StandardCommandsEnum.Exit);
				PrepareStringThenSend(command);
			}
		}

		/// <summary>
		/// Property indicating that the Home command is supported.
		/// </summary>
		public bool SupportsHome
		{
			get { return false; }
		}

		/// <summary>
		/// Method to send the home command to the Device.
		/// </summary>
		public void Home()
		{
			if (!Initialized)
			{
				return;
			}

			if (!SupportsHome)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem : The command Home is not supported");
				return;
			}
			if (SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum.Home))
			{
				var command = new CommandSet("Home", "Home",
							 CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, StandardCommandsEnum.Home);

				PrepareStringThenSend(command);
			}
		}

		/// <summary>
		/// Property indicating that the Menu command is supported.
		/// </summary>
		public bool SupportsMenu { get { return false; } }

		/// <summary>
		/// Method to send the menu command to the Device.
		/// </summary>
		public void Menu()
		{
			if (!Initialized)
			{
				return;
			}

			if (!SupportsMenu)
			{
				SecuritySystemProtocol.LogMessage("SecuritySystem : The command Menu is not supported");
				return;
			}
			if (SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum.Menu))
			{
				var command = new CommandSet("Menu", "Menu",
							   CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, StandardCommandsEnum.Menu);

				PrepareStringThenSend(command);
			}
		}

		#endregion Navigation

		#region Keypad Status Text

		private string _statusText;

		/// <summary>
		/// Flag that indicates that Status Text Feedback is supported.
		/// </summary>
		public bool SupportsKeypadStatusText { get; set; }

		#endregion

		#region IEmulatedSecuritySystemKeypad Implementation

		/// <summary>
		/// Gets a list of security system keypad function buttons
		/// </summary>
		public SecuritySystemKeypadFunctionButton[] FunctionButtons { get; protected set; }


		/// <summary>
		/// Gets the list of security system keypad leds
		/// </summary>
		public SecuritySystemKeypadLed[] Leds { get; protected set; }

		/// <summary>
		/// Gets security system keypad status text
		/// </summary>
		public string StatusText { get; private set; }

		/// <summary>
		/// Gets whether security system keypad supports function buttons or not
		/// </summary>
		public bool SupportsFunctionButtons
		{
			get { return FunctionButtons.Length > 0; }
		}

		/// <summary>
		/// Gets whether security system keypad supports leds or not
		/// </summary>
		public bool SupportsLeds { get; private set; }

		/// <summary>
		/// Gets security system keypad supports textual display
		/// </summary>
		public bool SupportsTextualDisplay { get { return false; } }

		/// <summary>
		/// Event gets raised as a result of keypad text changed being issued to the security system
		/// <see cref=""/> 
		/// </summary>

		public event EventHandler<SecuritySystemKeypadTextChangedEventArgs> SecuritysystemKeypadTextChanged;


		/// <summary>
		/// Trigger the security system keypad function button
		/// </summary>
		/// <param name="functionButtonId"></param>
		public void TriggerFunctionButton(int buttonNumber)
		{
			if (!Initialized)
			{
				return;
			}

			if (SecuritySystemProtocol.CommandsDictionary.ContainsKey(StandardCommandsEnum.FunctionButton1))
			{
				StandardCommandsEnum commandEnum = StandardCommandsEnum.FunctionButton1;
				switch (buttonNumber)
				{
					case 1:
						break;
					case 2:
						commandEnum = StandardCommandsEnum.FunctionButton2;
						break;
					case 3:
						commandEnum = StandardCommandsEnum.FunctionButton3;
						break;
					case 4:
						commandEnum = StandardCommandsEnum.FunctionButton4;
						break;
					case 5:
						commandEnum = StandardCommandsEnum.FunctionButton5;
						break;
					case 6:
						commandEnum = StandardCommandsEnum.FunctionButton6;
						break;
					case 7:
						commandEnum = StandardCommandsEnum.FunctionButton7;
						break;
					case 8:
						commandEnum = StandardCommandsEnum.FunctionButton8;
						break;
					default:
						return;
				}


				var command = new CommandSet(commandEnum.ToString(), commandEnum.ToString(),
						   CommonCommandGroupType.Unknown, null, false, CommandPriority.Low, commandEnum);
				PrepareStringThenSend(command);
			}
		}


		#endregion

		#region Private Method

		private void SecuritySystemProtocolOnStateChange(SecuritySystemStateObjects securitySystemStateObjects, object changedObject)
		{
			switch (securitySystemStateObjects)
			{
				case SecuritySystemStateObjects.KeypadStatusText:
					var newText = (string)changedObject;
					if (newText != StatusText)
					{
						StatusText = newText;
						if (SecuritysystemKeypadTextChanged != null)
							SecuritysystemKeypadTextChanged(this, new SecuritySystemKeypadTextChangedEventArgs { KeypadText = StatusText });
					}
					break;
				case SecuritySystemStateObjects.KeypadLeds:
					var newLed = (ISecuritySystemKeypadLed)changedObject;
					if (Leds != null)
					{
						if (Leds[newLed.Index].State != newLed.State)
						{
							Leds[newLed.Index].State = newLed.State;
						}
					}
					break;
				case SecuritySystemStateObjects.AuxiliaryButtonLedState:
					var functionButton = (SecuritySystemKeypadFunctionButton)changedObject;
					if (FunctionButtons != null)
					{
						if (FunctionButtons[functionButton.FunctionId].Led.State != functionButton.Led.State)
						{
							FunctionButtons[functionButton.FunctionId].Led.State = functionButton.Led.State;
						}
					}
					break;
			}
		}

		#endregion

	}
}