// For Basic SIMPL# Classes
// For Basic SIMPL#Pro classes

using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.DeviceTypes.Display;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Newtonsoft.Json;

namespace SamplePairedDeviceDriver.Devices
{
	/// <summary>
	/// Paired device driver class represents the device capabilities.
	/// <para>Depending on the device functionality and communication, this driver can have Protocol and Transport.</para>
	/// </summary>
	public class SamplePairedDevice : ABasicVideoDisplay
	{
		private readonly string _id;
		private readonly string _name;
		#region Properties

		/// <summary>
		/// Unique identifier for the device.
		/// </summary>
		public new string Id
		{
			get { return _id; }
		}

		/// <summary>
		/// Friendly name of the device.
		/// </summary>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// User friendly description of the device.
		/// </summary>
		public new string Description
		{
			get { return "Device description for Sample paired device"; }
		}

		/// <summary>
		/// The name of the manufacturer.
		/// </summary>
		public new string Manufacturer
		{
			get { return "Crestron"; }
		}

		/// <summary>
		/// Name of the device model.
		/// </summary>
		public string Model
		{
			get { return "Sample PairedDevice"; }
		}

		/// <summary>
		/// Device type of the device. 
		/// <para>If the paired device is not an extensin=on device, then the device type should <see cref="DeviceTypes"/>
		/// otherwise a custom device type can be specified.</para>
		/// </summary>
		public string DeviceType
		{
			get { return "Projector"; }
		}

		/// <summary>
		/// Device subtype of the device.
		/// </summary>
		public string DeviceSubtype
		{
			get { return "Paired Device SubType"; }
		}

		public override CType AbstractClassType
		{
			get { return GetType();}
		}

		#endregion

		#region Initialization

		public SamplePairedDevice(string id, string name)
		{
			_id = id;
			_name = name;
		}

		#endregion Initialization

		#region Base Members

		/// <summary>
		/// Sets connection status on the device base on the connection status from gateway
		/// </summary>
		/// <param name="connection"></param>
		public void SetConnectionStatus(bool connection)
		{
			Connected = connection;
		}

		#endregion
	}
}

