using System;
using SamplePairedDeviceDriver.Devices;

namespace Crestron.Samples.Platforms
{
	internal class SampleDeviceFactoryStatusEventArgs : EventArgs
	{
		public DeviceStatus Status { get; private set; }

		public SamplePairedDevice Device { get; private set; }

		public SampleDeviceFactoryStatusEventArgs(DeviceStatus status, SamplePairedDevice device)
		{
			Status = status;
			Device = device;
		}
	}
}