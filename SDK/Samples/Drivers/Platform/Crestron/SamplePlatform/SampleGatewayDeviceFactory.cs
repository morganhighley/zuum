using System;
using Crestron.SimplSharp;
using SamplePairedDeviceDriver.Devices;

namespace Crestron.Samples.Platforms
{
	public class SampleGatewayDeviceFactory
	{
		internal event EventHandler<SampleDeviceFactoryStatusEventArgs> DeviceStatusChanged;

		private static readonly SampleGatewayDeviceFactory StaticInstance = new SampleGatewayDeviceFactory();
		private readonly CTimer _deviceStateTimer;
		private const uint DiscoverDeviceTime = 5000;
		private DeviceFactoryState _deviceFactoryState = DeviceFactoryState.None;
		private const string DeviceIdFormat = "SamplePairedDevice_{0}";
		private const string DeviceNameFormat = "Sample Paired Device Name_{0}";
		private const uint NoOfDevicesToCreate = 3;

		public static SampleGatewayDeviceFactory Instance
		{
			get { return StaticInstance; }
		}

		private SampleGatewayDeviceFactory()
		{
			_deviceStateTimer = new CTimer(DeviceStateTimerCallback, Timeout.Infinite);
		}

		public void DiscoverDevices()
		{
			_deviceFactoryState = DeviceFactoryState.DiscoverDevices;
			_deviceStateTimer.Reset(DiscoverDeviceTime);
		}

		private void DeviceStateTimerCallback(object userSpecific)
		{
			switch (_deviceFactoryState)
			{
				case DeviceFactoryState.DiscoverDevices:
				{
					// Create paired devices to gateway.
					for (int i = 1; i <= NoOfDevicesToCreate; i++)
					{
						var id = string.Format(DeviceIdFormat, i);
						var name = string.Format(DeviceNameFormat, i);

						var pairedDevice = new SamplePairedDevice(id, name);

						if (DeviceStatusChanged != null)
						{
							DeviceStatusChanged(this,
								new SampleDeviceFactoryStatusEventArgs(DeviceStatus.Added, pairedDevice));
						}
					}

					break;
				}
			}
		}

		public void Dispose()
		{
			if (_deviceStateTimer != null)
			{
				_deviceStateTimer.Stop();
				_deviceStateTimer.Dispose();
			}
			_deviceFactoryState = DeviceFactoryState.None;
		}

		enum DeviceFactoryState
		{
			None,
			DiscoverDevices,
			Active,
			Inactive,
		}
	}

	internal enum DeviceStatus
	{
		Added,
		Removed,
		Updated,
	}
}