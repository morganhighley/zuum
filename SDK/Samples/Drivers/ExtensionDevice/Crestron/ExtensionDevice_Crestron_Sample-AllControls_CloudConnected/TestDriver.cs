using System;
using System.Collections.Generic;
using Crestron.RAD.Common;
using Crestron.RAD.Common.Attributes.Programming;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.Common.Interfaces.ExtensionDevice;
using Crestron.RAD.DeviceTypes.ExtensionDevice;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes

namespace TestDriver
{
	public class TestDriver : AExtensionDevice, ICloudConnected
	{
		private Dictionary<string, IPropertyValue> _properties = new Dictionary<string, IPropertyValue>();
		private ObjectList _list;
		private ClassDefinition _listclassDefinition;

		private bool deviceState;

		public TestDriver()
		{
			//Tile
			_properties["titlestate"] = CreateProperty<string>(new PropertyDefinition("titlestate", "titlestate", DevicePropertyType.String));
			_properties["demodeviceicon"] = CreateProperty<string>(new PropertyDefinition("demodeviceicon", "demodeviceicon", DevicePropertyType.String));
			_properties["secondaryicon"] = CreateProperty<string>(new PropertyDefinition("secondaryicon", "secondaryicon", DevicePropertyType.String));

			//Segmented Slider
			var segmentedSliderAvailableValues = new List<IPropertyAvailableValue>();
			segmentedSliderAvailableValues.Add(new PropertyAvailableValue<string>("1", DevicePropertyType.String, "Txt1", null));
			segmentedSliderAvailableValues.Add(new PropertyAvailableValue<string>("2", DevicePropertyType.String, "Txt2", null));
			segmentedSliderAvailableValues.Add(new PropertyAvailableValue<string>("3", DevicePropertyType.String, "Txt3", null));
			segmentedSliderAvailableValues.Add(new PropertyAvailableValue<string>("4", DevicePropertyType.String, "Txt4", null));
			_properties["segmentedMainPageSliderValue"] = CreateProperty<string>(new PropertyDefinition("segmentedMainPageSliderValue", "segmentedMainPageSliderValue", DevicePropertyType.String, segmentedSliderAvailableValues));

			//ToggleSlider
			_properties["toggleMainPageToggleValue"] = CreateProperty<bool>(new PropertyDefinition("toggleMainPageToggleValue", "toggleMainPageToggleValue", DevicePropertyType.Boolean));
			_properties["toggleMainPageSliderValue"] = CreateProperty<int>(new PropertyDefinition("toggleMainPageSliderValue", "toggleMainPageSliderValue", DevicePropertyType.Int32, 0, 100, 1));

			//Toggle 1
			_properties["toggleMainPageNoDotValue"] = CreateProperty<bool>(new PropertyDefinition("toggleMainPageNoDotValue", "toggleMainPageNoDotValue", DevicePropertyType.Boolean));
			_properties["toggleMainPageNoDotSecLabel"] = CreateProperty<string>(new PropertyDefinition("toggleMainPageNoDotSecLabel", "toggleMainPageNoDotSecLabel", DevicePropertyType.String));

			//Toggle2
			_properties["toggleMainPageWithDotValue"] = CreateProperty<bool>(new PropertyDefinition("toggleMainPageWithDotValue", "toggleMainPageWithDotValue", DevicePropertyType.Boolean));
			_properties["toggleMainPageWithDotSecLabel"] = CreateProperty<string>(new PropertyDefinition("toggleMainPageWithDotSecLabel", "toggleMainPageWithDotSecLabel", DevicePropertyType.String));

			//Status and button
			_properties["statusAndButtonMainPageStatus"] = CreateProperty<string>(new PropertyDefinition("statusAndButtonMainPageStatus", "statusAndButtonMainPageStatus", DevicePropertyType.String));
			_properties["statusAndButtonMainPageAlert"] = CreateProperty<string>(new PropertyDefinition("statusAndButtonMainPageAlert", "statusAndButtonMainPageAlert", DevicePropertyType.String));

			//Buttons
			_properties["textDisplay2ButtonsLabel"] = CreateProperty<string>(new PropertyDefinition("textDisplay2ButtonsLabel", "textDisplay2ButtonsLabel", DevicePropertyType.String));
			_properties["textDisplay4ButtonsLabel"] = CreateProperty<string>(new PropertyDefinition("textDisplay4ButtonsLabel", "textDisplay4ButtonsLabel", DevicePropertyType.String));
			_properties["textDisplay5ButtonsLabel"] = CreateProperty<string>(new PropertyDefinition("textDisplay5ButtonsLabel", "textDisplay5ButtonsLabel", DevicePropertyType.String));
			_properties["textDisplayListButtonSelectedLabel"] = CreateProperty<string>(new PropertyDefinition("textDisplayListButtonSelectedLabel", "textDisplayListButtonSelectedLabel", DevicePropertyType.String));

			//Selector Button
			_properties["selectorButtonValue"] = CreateProperty<string>(new PropertyDefinition("selectorButtonValue", "selectorButtonValue", DevicePropertyType.String, segmentedSliderAvailableValues));

			//List Button
			_listclassDefinition = CreateClassDefinition("item");
			_listclassDefinition.AddProperty(new PropertyDefinition("Name", "Name", DevicePropertyType.String));
			_list = CreateList(new PropertyDefinition("listButtonSource", "listButtonSource", DevicePropertyType.ObjectList, _listclassDefinition));
			CreateItemList();

			//Raise Lower with Text
			_properties["raiseLowerWithTextValue"] = CreateProperty<int>(new PropertyDefinition("raiseLowerWithTextValue", "raiseLowerWithTextValue", DevicePropertyType.Int32, 0, 100, 1));
			_properties["raiseLowerWithTextFormat"] = CreateProperty<string>(new PropertyDefinition("raiseLowerWithTextFormat", "raiseLowerWithTextFormat", DevicePropertyType.String));

			//Text Entry
			_properties["textEntryErrorText"] = CreateProperty<string>(new PropertyDefinition("textEntryErrorText", "textEntryErrorText", DevicePropertyType.String));

			//Checkbox
			_properties["checkboxValue"] = CreateProperty<bool>(new PropertyDefinition("checkboxValue", "checkboxValue", DevicePropertyType.Boolean));
			_properties["checkboxSecLabel"] = CreateProperty<string>(new PropertyDefinition("checkboxSecLabel", "checkboxSecLabel", DevicePropertyType.String));

			//Pad Text displays
			_properties["textDpadButtonsLabel"] = CreateProperty<string>(new PropertyDefinition("textDpadButtonsLabel", "textDpadButtonsLabel", DevicePropertyType.String));
			_properties["textKeyPadButtonsLabel"] = CreateProperty<string>(new PropertyDefinition("textKeyPadButtonsLabel", "textKeyPadButtonsLabel", DevicePropertyType.String));

			//Radial Gauge
			_properties["radialGuageValue"] = CreateProperty<int>(new PropertyDefinition("radialGuageValue", "radialGuageValue", DevicePropertyType.Int32, 0, 100, 1));

			//Thermostat
			_properties["heatModeEnabled"] = CreateProperty<bool>(new PropertyDefinition("heatModeEnabled", "heatModeEnabled", DevicePropertyType.Boolean));
			_properties["coolModeEnabled"] = CreateProperty<bool>(new PropertyDefinition("coolModeEnabled", "coolModeEnabled", DevicePropertyType.Boolean));
			_properties["autoModeEnabled"] = CreateProperty<bool>(new PropertyDefinition("autoModeEnabled", "autoModeEnabled", DevicePropertyType.Boolean));
			_properties["temperatureUnits"] = CreateProperty<string>(new PropertyDefinition("temperatureUnits", "temperatureUnits", DevicePropertyType.String));
			_properties["setpointAuto"] = CreateProperty<int>(new PropertyDefinition("setpointAuto", "setpointAuto", DevicePropertyType.Int32, 45, 90, 1));
			_properties["setpointCool"] = CreateProperty<int>(new PropertyDefinition("setpointCool", "setpointCool", DevicePropertyType.Int32, 65, 90, 1));
			_properties["setpointHeat"] = CreateProperty<int>(new PropertyDefinition("setpointHeat", "setpointHeat", DevicePropertyType.Int32, 45, 80, 1));
			_properties["modeIcon"] = CreateProperty<string>(new PropertyDefinition("modeIcon", "modeIcon", DevicePropertyType.String));
			_properties["currentTemperature"] = CreateProperty<int>(new PropertyDefinition("currentTemperature", "currentTemperature", DevicePropertyType.Int32));
			_properties["currentTemperatureLabel"] = CreateProperty<string>(new PropertyDefinition("currentTemperatureLabel", "currentTemperatureLabel", DevicePropertyType.String));

			//Raise Lower with Toggle
			_properties["raiselowerwithtextToggleValue"] = CreateProperty<int>(new PropertyDefinition("raiselowerwithtextToggleValue", "raiselowerwithtextToggleValue", DevicePropertyType.Int32, 0, 100, 1));

			Connected = true;
			Intitialize();
		}

		private void CreateItemList()
		{
			for (int i = 1; i <= 10; i++)
			{
				var tempObject = CreateObject(_listclassDefinition);
				tempObject.GetValue<string>("Name").Value = "Item " + i;
				_list.AddObject(tempObject);
			}
		}

		private void Intitialize()
		{
			CrestronConsole.PrintLine("Rad Driver: Initialize Enter ~.");
			deviceState = false;
			((PropertyValue<string>)_properties["titlestate"]).Value = deviceState == true ? "Running" : "Stopped";
            ((PropertyValue<string>)_properties["demodeviceicon"]).Value = deviceState == true ? "icSprinklersOn" : "icSprinklersOff";
            ((PropertyValue<string>)_properties["secondaryicon"]).Value = deviceState == true ? "icRemoteButtonGreen" : "icRemoteButtonRed";
            ((PropertyValue<string>)_properties["segmentedMainPageSliderValue"]).Value = "1";

            ((PropertyValue<bool>)_properties["toggleMainPageToggleValue"]).Value = false;
            ((PropertyValue<int>)_properties["toggleMainPageSliderValue"]).Value = 25;

            ((PropertyValue<bool>)_properties["toggleMainPageNoDotValue"]).Value = false;
            ((PropertyValue<string>)_properties["toggleMainPageNoDotSecLabel"]).Value = "Off";

            ((PropertyValue<bool>)_properties["toggleMainPageWithDotValue"]).Value = false;
            ((PropertyValue<string>)_properties["toggleMainPageWithDotSecLabel"]).Value = "Off";
            ((PropertyValue<string>)_properties["statusAndButtonMainPageStatus"]).Value = "Sample Text";
            ((PropertyValue<string>)_properties["statusAndButtonMainPageAlert"]).Value = "icAlertRegular";

            ((PropertyValue<string>)_properties["textDisplay2ButtonsLabel"]).Value = "";
            ((PropertyValue<string>)_properties["textDisplay4ButtonsLabel"]).Value = "";
            ((PropertyValue<string>)_properties["textDisplay5ButtonsLabel"]).Value = "";
            ((PropertyValue<string>)_properties["selectorButtonValue"]).Value = "1";

            ((PropertyValue<string>)_properties["textDisplayListButtonSelectedLabel"]).Value = "";

            ((PropertyValue<int>)_properties["raiseLowerWithTextValue"]).Value = 25;
            ((PropertyValue<string>)_properties["raiseLowerWithTextFormat"]).Value = "%s %";

            ((PropertyValue<string>)_properties["textEntryErrorText"]).Value = "Sample Error Text";

            ((PropertyValue<bool>)_properties["checkboxValue"]).Value = false;
            ((PropertyValue<string>)_properties["checkboxSecLabel"]).Value = "Unchecked";
			CrestronConsole.PrintLine("Rad Driver: Initialize - 20");
            ((PropertyValue<string>)_properties["textDpadButtonsLabel"]).Value = "";
            ((PropertyValue<string>)_properties["textKeyPadButtonsLabel"]).Value = "";

            ((PropertyValue<int>)_properties["radialGuageValue"]).Value = 60;

            ((PropertyValue<bool>)_properties["heatModeEnabled"]).Value = true;
            ((PropertyValue<bool>)_properties["coolModeEnabled"]).Value = true;
            ((PropertyValue<bool>)_properties["autoModeEnabled"]).Value = false;
            ((PropertyValue<string>)_properties["temperatureUnits"]).Value = "Fahrenheit";
            ((PropertyValue<int>)_properties["setpointAuto"]).Value = 70;
            ((PropertyValue<int>)_properties["setpointCool"]).Value = 72;
            ((PropertyValue<int>)_properties["setpointHeat"]).Value = 68;
            ((PropertyValue<string>)_properties["modeIcon"]).Value = "icCoolingRegular";
            ((PropertyValue<int>)_properties["currentTemperature"]).Value = 70;
            ((PropertyValue<string>)_properties["currentTemperatureLabel"]).Value = "70";
            ((PropertyValue<int>)_properties["raiselowerwithtextToggleValue"]).Value = 30;

			Commit();
		}
		protected override IOperationResult DoCommand(string command, string[] parameters)
		{
			switch (command)
			{
				case "toggledevice":
					deviceState = !deviceState;
                    ((PropertyValue<string>)_properties["titlestate"]).Value = deviceState == true ? "Running" : "Stopped";
                    ((PropertyValue<string>)_properties["demodeviceicon"]).Value = deviceState == true ? "icSprinklersOn" : "icSprinklersOff";
                    ((PropertyValue<string>)_properties["secondaryicon"]).Value = deviceState == true ? "icRemoteButtonGreen" : "icRemoteButtonRed";
                    ((PropertyValue<string>)_properties["statusAndButtonMainPageStatus"]).Value = deviceState == true ? "Running" : "Stopped";
					break;
				case "start":
					deviceState = !deviceState;
                    ((PropertyValue<string>)_properties["titlestate"]).Value = deviceState == true ? "Running" : "Stopped";
                    ((PropertyValue<string>)_properties["demodeviceicon"]).Value = deviceState == true ? "icSprinklersOn" : "icSprinklersOff";
                    ((PropertyValue<string>)_properties["secondaryicon"]).Value = deviceState == true ? "icRemoteButtonGreen" : "icRemoteButtonRed";
                    ((PropertyValue<string>)_properties["statusAndButtonMainPageStatus"]).Value = deviceState == true ? "Running" : "Stopped";
					break;
				case "plus":
                    ((PropertyValue<string>)_properties["textDisplay2ButtonsLabel"]).Value = "+";
					break;
				case "minus":
                    ((PropertyValue<string>)_properties["textDisplay2ButtonsLabel"]).Value = "-";
					break;
				case "up":
                    ((PropertyValue<string>)_properties["textDisplay4ButtonsLabel"]).Value = "Up";
					break;
				case "down":
                    ((PropertyValue<string>)_properties["textDisplay4ButtonsLabel"]).Value = "Down";
					break;
				case "left":
                    ((PropertyValue<string>)_properties["textDisplay4ButtonsLabel"]).Value = "Left";
					break;
				case "right":
                    ((PropertyValue<string>)_properties["textDisplay4ButtonsLabel"]).Value = "Right";
					break;
				case "up1":
                    ((PropertyValue<string>)_properties["textDisplay5ButtonsLabel"]).Value = "Up";
					break;
				case "down1":
                    ((PropertyValue<string>)_properties["textDisplay5ButtonsLabel"]).Value = "Down";
					break;
				case "left1":
                    ((PropertyValue<string>)_properties["textDisplay5ButtonsLabel"]).Value = "Left";
					break;
				case "right1":
                    ((PropertyValue<string>)_properties["textDisplay5ButtonsLabel"]).Value = "Right";
					break;
				case "select":
                    ((PropertyValue<string>)_properties["textDisplay5ButtonsLabel"]).Value = "OK";
					break;
				case "listItemSelected":
                    ((PropertyValue<string>)_properties["textDisplayListButtonSelectedLabel"]).Value = parameters[0];
					break;
				case "dpadUp":
                    ((PropertyValue<string>)_properties["textDpadButtonsLabel"]).Value = "Up";
					break;
				case "dpadDown":
                    ((PropertyValue<string>)_properties["textDpadButtonsLabel"]).Value = "Down";
					break;
				case "dpadLeft":
                    ((PropertyValue<string>)_properties["textDpadButtonsLabel"]).Value = "Left";
					break;
				case "dpadRight":
                    ((PropertyValue<string>)_properties["textDpadButtonsLabel"]).Value = "Right";
					break;
				case "dpadCenter":
                    ((PropertyValue<string>)_properties["textDpadButtonsLabel"]).Value = "Center";
					break;
				case "keyPressed":
                    ((PropertyValue<string>)_properties["textKeyPadButtonsLabel"]).Value = parameters[0];
					break;
				default:
					ErrorLog.Warn("DemoExtensionDevice: Unhandled command: " + command);
					break;
			}

			Commit();
			return new OperationResult(OperationResultCode.Success);
		}

		protected override IOperationResult SetDriverPropertyValue<T>(string propertyKey, T value)
		{
            ((PropertyValue<T>)_properties[propertyKey]).Value = value;

			switch (propertyKey)
			{
				case "toggleMainPageNoDotValue":
                    ((PropertyValue<string>)_properties["toggleMainPageNoDotSecLabel"]).Value = value.Equals(true) ? "On" : "Off";
					break;
				case "toggleMainPageWithDotValue":
                    ((PropertyValue<string>)_properties["toggleMainPageWithDotSecLabel"]).Value = value.Equals(true) ? "On" : "Off";
					break;
				case "checkboxValue":
                    ((PropertyValue<string>)_properties["checkboxSecLabel"]).Value = value.Equals(true) ? "Checked" : "Unchecked";
					break;
			}
			Commit();
			return new OperationResult(OperationResultCode.Success);
		}

		protected override IOperationResult SetDriverPropertyValue<T>(string objectId, string propertyKey, T value)
		{
			return new OperationResult(OperationResultCode.Success);
		}

		public void Initialize()
		{

        }

        #region ProgrammableOperations

        /// <summary>
        /// To get a method to appear as an operation in sequences, simply decorate the method with the <see cref="ProgrammableOperationAttribute"/>.
        /// By default the name of the operation in sequences will have the same name as the method.
        /// </summary>
        [ProgrammableOperation]
	    public void Method1()
	    {
	        
	    }

	    /// <summary>
	    /// To get a property to appear in sequences, simply decorate the property with the <see cref="ProgrammableOperationAttribute"/>.
        /// By default the name of the operation in sequences will have the same name as the property.
	    /// </summary>
        /// <remarks>
        /// The property must have a setter.
        /// </remarks>
	    [ProgrammableOperation]
	    public int Property1 { get; set; }

        /// <summary>
        /// To customize the operation name for a method in sequences, use the <see cref="ProgrammableOperationAttribute(string)"/>
        /// </summary>
        /// <remarks>
        /// To localize the operation name, prepend the operation name with a ^ and add the translation to the applicable translation file (ex: en-US.json).
        /// </remarks>
        [ProgrammableOperation("FooMethod")]
	    public void Method2()
	    {
	        
	    }

        /// <summary>
        /// To customize the operation name for a property in sequences, use the <see cref="ProgrammableOperationAttribute(string)"/>
        /// </summary>
        /// <remarks>
        /// To localize the operation name, prepend the operation name with a ^ and add the translation to the applicable translation file (ex: en-US.json).
        /// </remarks>
        [ProgrammableOperation("^FooProperty")]
        public int Property2 { get; set; }

        /// <summary>
        /// Methods with parameters can also appear as an operation in sequences.  
        /// By default, the parameter names given will be used in the sequence parameter dialog.
        /// </summary>
        [ProgrammableOperation]
	    public void Method3(
            int time,
            double length)
	    {
	        
	    }

        /// <summary>
        /// To customize parameter names for a method, use the <see cref="DisplayAttribute(string)"/>
        /// </summary>
        /// <remarks>
        /// To localize parameter names, prepend the parameter name with a ^ and add the translation to the applicable translation file (ex: en-US.json).
        /// </remarks>
        [ProgrammableOperation]
        public void Method4(
            [Display("Time")]
            int time,
            [Display("^Length")]
            double length)
        {
            
        }

        /// <summary>
        /// To display a unit next to a parameter in the sequence parameter dialog, use the <see cref="UnitAttribute(Unit)"/>
        /// </summary>
        /// <remarks>
        /// Units are displayed in parentheses next to the parameter name in the sequence parameter dialog: Time (s)
        /// Multiple attributes can be used for a single parameter.
        /// </remarks>
        [ProgrammableOperation]
        public void Method5(
            [Display("Time")]
            [Unit(Unit.Seconds)]
            int time,
            [Display("^Length")]
            double length)
        {

        }

        /// <summary>
        /// To specify a default parameter in the sequence parameter dialog, use the <see cref="DefaultAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Overloaded constructors exist for parameters of type int, double, string, and bool.
        /// A parameter can also be marked as read-only which results in the method always being executed with the value specified.
        /// </remarks>
        [ProgrammableOperation]
	    public void Method6(
	        [Default(5)] 
            int time,
            [Default(1.1, true)]
            double length)
	    {
	        
	    }

        /// <summary>
        /// To specify a default value for a property in the sequence parameter dialog, use the <see cref="DefaultAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Overloaded constructors exist for parameters of type int, double, string, and bool.
        /// A parameter can also be marked as read-only which results in the method always being executed with the value specified.
        /// </remarks>
        [ProgrammableOperation]
        [Default(3)]
        public int Property6 { get; set; }

        /// <summary>
        /// To specify a minimum value for a parameter in the sequence parameter dialog, use the <see cref="MinAttribute"/>.
        /// If a value less than the minimum specified is entered in the sequence parameter dialog, a validation error will occur.
        /// </summary>
        /// <remarks>
        /// Overloaded constructors exist for parameters of type int and double.  
        /// If the <see cref="MinAttribute"/> is used on a parameter of type <see cref="string"/>, the minimum value specified will
        /// apply against the length of the string.
        /// </remarks>
        [ProgrammableOperation]
	    public void Method7(
	        [Min(2)]
            int time)
	    {
	        
	    }

        /// <summary>
        /// To specify a minimum value for a property in the sequence parameter dialog, use the <see cref="MinAttribute"/>.
        /// If a value less than the minimum specified is entered in the sequence parameter dialog, a validation error will occur.
        /// </summary>
        /// <remarks>
        /// Overloaded constructors exist for parameters of type int and double.  
        /// If the <see cref="MinAttribute"/> is used on a property of type <see cref="string"/>, the minimum value specified will
        /// apply against the length of the string.
        /// </remarks>
        [ProgrammableOperation]
        [Min(10)]
        public string Property7 { get; set; }

        /// <summary>
        /// To specify a maximum value for a parameter in the sequence parameter dialog, use the <see cref="MaxAttribute"/>.
        /// If a value greater than the maximum specified is entered in the sequence parameter dialog, a validation error will occur.
        /// </summary>
        /// <remarks>
        /// Overloaded constructors exist for parameters of type int and double.  
        /// If the <see cref="MaxAttribute"/> is used on a property of type <see cref="string"/>, the maximum value specified will
        /// apply against the length of the string.
        /// </remarks>
        [ProgrammableOperation]
        public void Method8(
            [Max(10)]
            int time)
        {

        }

        /// <summary>
        /// To specify a maximum value for a property in the sequence parameter dialog, use the <see cref="MaxAttribute"/>.
        /// If a greater than the maximum specified is entered in the sequence parameter dialog, a validation error will occur.
        /// </summary>
        /// <remarks>
        /// Overloaded constructors exist for parameters of type int and double.  
        /// If the <see cref="MaxAttribute"/> is used on a property of type <see cref="string"/>, the maximum value specified will
        /// apply against the length of the string.
        /// </remarks>
        [ProgrammableOperation]
        [Max(10)]
        public int Property8 { get; set; }

        /// <summary>
        /// To specify a list of values for a parameter in the sequence parameter dialog that are known at compile time,
        /// use the <see cref="AvailableValuesAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Overloaded constructors exist for parameters of type int, double, and string.
        /// </remarks>
        [ProgrammableOperation]
	    public void Method9(
            [AvailableValues(5, 10, 15, 20)]
            int time)
	    {
	        
	    }

        /// <summary>
        /// To specify a list of values for a property in the sequence parameter dialog that are known at compile time,
        /// use the <see cref="AvailableValuesAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Overloaded constructors exist for parameters of type int, double, and string.
        /// </remarks>
        [ProgrammableOperation]
        [AvailableValues(5, 10, 15, 20)]
        public double Property9 { get; set; }

        /// <summary>
        /// To specify a list of values for a parameter or propery in the sequence parameter dialog that are not known at compile time,
        /// use the <see cref="DynamicAvailableValuesAttribute"/>.  Sequences will execute the given method or property prior to displaying
        /// the parameter dialog.
        /// </summary>
        /// <remarks>
        /// The method or property specified must return an <see cref="IEnumerable{T}"/>.
        /// The type name specified must be the name of the class or interface in which the method or property returning the IEnumerable 
        /// belongs to.
        /// Use the optional path property using dot notation to specify the given method or property resides on a nested object(s) - "Foo.SubFoo"
        /// </remarks>
        [ProgrammableOperation]
	    public void Method10(
	        [DynamicAvailableValues("TestDriver", "GetTimeValues", OperationType.Method)]
            int time)
	    {
	        
	    }

        /// <summary>
        /// Method that will be called by sequences before displaying the sequence parameter dialog for Method10.
        /// </summary>
        private List<int> GetTimeValues()
	    {
	        // Applicable business logic goes here to build the returning list below.
            return new List<int> {10, 20, 30};
	    }

	    
        /// <summary>
        /// To only show a method or property in sequences depending on a business condition, use the 
        /// <see cref="IsSupportedAttribute"/>
        /// </summary>
        /// <remarks>
        /// The method or property specified must return a <see cref="bool"/>.
        /// The type name specified must be the name of the class or interface in which the method or property returning the bool 
        /// belongs to.
        /// Use the optional path property using dot notation to specify the given method or property resides on a nested object(s) - "Foo.SubFoo"
        /// </remarks>
        [ProgrammableOperation]
        [IsSupported("TestDriver", "IsMethod11Supported", OperationType.Property)]
        public void Method11()
	    {
            	        
	    }

	    public bool IsMethod11Supported
	    {
            // Applicable business logic goes here to determine whether or not
            // Method11 is supported.
	        get { return true; }
	    }

	    #endregion ProgrammableOperations

        #region ProgrammableEvents

	    /// <summary>
	    /// To expose an event on a device as a programmable event, simply decorate the event with the <see cref="ProgrammableEventAttribute"/>.
	    /// By default the programmable event name will have the same name as the event.
	    /// </summary>
	    [ProgrammableEvent]
	    public event Action Event1;

        /// <summary>
        /// To customize the name of an event, use the <see cref="ProgrammableEventAttribute(string)"/>.
        /// </summary>
        /// <remarks>
        /// To localize the event name, prepend the event name with a ^ and add the translation to the applicable translation file (ex: en-US.json).
        /// </remarks>
        [ProgrammableEvent("FooEvent")]
	    public event Action Event2;

        /// <summary>
        /// To display a warning message about loops when a certain operation(s) is added to a sequence while programming this event, use 
        /// the <see cref="TriggeredByAttribute"/>.
        /// </summary>
        /// <remarks>
        /// The operation names specified must be <see cref="ProgrammableOperationAttribute"/>s.  The names specified must be the name of the method/property or
        /// the display name of the operation.
        /// </remarks>
        [ProgrammableEvent("^Event3")]
        [TriggeredBy("Method1","^FooProperty")]
	    public event Action Event3;

        /// <summary>
        /// To only show an event depending on a business condition, use the <see cref="IsSupportedAttribute"/>.
        /// </summary>
        /// <remarks>
        /// The method or property specified must return a <see cref="bool"/>.
        /// The type name specified must be the name of the class or interface in which the method or property returning the bool 
        /// belongs to.
        /// Use the optional path property using dot notation to specify the given method or property resides on a nested object(s) - "Foo.SubFoo"
        /// </remarks>
        [ProgrammableEvent]
        [IsSupported("TestDriver", "IsEvent4Supported", OperationType.Method)]
	    public event Action Event4;

        public bool IsEvent4Supported()
        {
            // Applicable business logic goes here to determine whether or not
            // Event4 is supported.
            return false;
        }

	    #endregion ProgrammableEvents
	}
}
