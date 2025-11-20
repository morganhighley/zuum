using System;
using System.Collections.Generic;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.DeviceTypes.Gateway;
using Crestron.SimplSharp;
using SamplePairedDeviceDriver.Devices;

namespace Crestron.Samples.Platforms
{
	public class SampleGatewayProtocol : AGatewayProtocol
	{
		#region Fields

		private readonly Dictionary<string, SamplePairedDevice> _pairedDevices =
			new Dictionary<string, SamplePairedDevice>();

		#endregion

		#region Initialization

		public SampleGatewayProtocol(ISerialTransport transport, byte id)
			: base(transport, id)
		{
			ValidateResponse = GatewayValidateResponse;
		}

		#endregion

		#region Base Members

		protected override void ChooseDeconstructMethod(ValidatedRxData validatedData)
		{
			// This is responsible for parsing the response from device and take apropriate updates to driver and notify applications about the change.
			if (validatedData.Data == "DiscoverDevicesResponse")
			{
				SampleGatewayDeviceFactory.Instance.DiscoverDevices();
			}
		}

		protected override void ConnectionChangedEvent(bool connection)
		{
			base.ConnectionChangedEvent(connection);

			foreach (var samplePairedDevice in _pairedDevices.Values)
				samplePairedDevice.SetConnectionStatus(connection);
		}

		#endregion

		#region Public Members

		/// <summary>
		/// Connects driver to device. 
		/// <para>Call to this method establish communication with device. The connection status will be set based on the connection response from device.</para> 
		/// </summary>
		public void Connect()
		{
			SampleGatewayDeviceFactory.Instance.DeviceStatusChanged -= Factory_PairedDeviceStatusChanged;
			SampleGatewayDeviceFactory.Instance.DeviceStatusChanged += Factory_PairedDeviceStatusChanged;
			SendDeviceDiscoveryRequest();
		}

		/// <summary>
		/// Disconnects driver to device. 
		/// <para>Call to this method disconnects driver communication with device.</para> 
		/// </summary>
		public void Disconnect()
		{
			SampleGatewayDeviceFactory.Instance.DeviceStatusChanged -= Factory_PairedDeviceStatusChanged;
		}

		#endregion

		#region Private Members

		private void SendDeviceDiscoveryRequest()
		{
			var command = new CommandSet("DeviceDiscovery", "DiscoverDevices", CommonCommandGroupType.Other, null, false,
				CommandPriority.Normal, StandardCommandsEnum.NotAStandardCommand);

			SendCommand(command);
		}

		private void Factory_PairedDeviceStatusChanged(object sender, SampleDeviceFactoryStatusEventArgs args)
		{
			switch (args.Status)
			{
				case DeviceStatus.Added:
					AddSamplePairedDevice(args.Device);
					break;
				case DeviceStatus.Updated:
					UpdateSamplePairedDevice(args.Device);
					break;
				case DeviceStatus.Removed:
					RemovePairedDevice(args.Device);
					break;
			}
		}

		private void AddSamplePairedDevice(SamplePairedDevice pairedDevice)
		{
			// Set connection status on device if the device created after the gateway is online.
			pairedDevice.SetConnectionStatus(IsConnected);

			if (!_pairedDevices.ContainsKey(pairedDevice.Id))
			{
				var pairedDeviceInformation = new GatewayPairedDeviceInformation(pairedDevice.Id,
					pairedDevice.Name, pairedDevice.Description, pairedDevice.Manufacturer, pairedDevice.Model,
					pairedDevice.DeviceType,
					pairedDevice.DeviceSubtype);
				AddPairedDevice(pairedDeviceInformation, pairedDevice);
				_pairedDevices.Add(pairedDevice.Id, pairedDevice);
			}
		}

		private void UpdateSamplePairedDevice(SamplePairedDevice pairedDevice)
		{
			pairedDevice.SetConnectionStatus(IsConnected);
			if (_pairedDevices.ContainsKey(pairedDevice.Id))
			{
				var pairedDeviceInformation = new GatewayPairedDeviceInformation(pairedDevice.Id,
					pairedDevice.Name, pairedDevice.Description, pairedDevice.Manufacturer, pairedDevice.Model,
					pairedDevice.DeviceType,
					pairedDevice.DeviceSubtype);

				UpdatePairedDevice(pairedDevice.Id, pairedDeviceInformation);
			}
		}

		private void RemovePairedDevice(SamplePairedDevice pairedDevice)
		{
			if (_pairedDevices.ContainsKey(pairedDevice.Id))
			{
				RemovePairedDevice(pairedDevice.Id);
			}
		}

		private ValidatedRxData GatewayValidateResponse(string response, CommonCommandGroupType commandGroup)
		{
			return new ValidatedRxData(true, "DiscoverDevicesResponse");
		}

		#endregion

	}
}