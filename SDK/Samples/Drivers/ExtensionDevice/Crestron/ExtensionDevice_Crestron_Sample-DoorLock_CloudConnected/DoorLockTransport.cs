using Crestron.RAD.Common.Transports;

namespace ExtensionDevice_Crestron_Sample_DoorLock_CloudConnected
{
	public class DoorLockTransport : ATransportDriver
	{
		public DoorLockTransport()
		{
			IsConnected = true;
		}

		public override void SendMethod(string message, object[] paramaters)
		{
		}

		public override void Start()
		{
		}

		public override void Stop()
		{
		}
	}
}