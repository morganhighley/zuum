using System;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Transports;

namespace ExtensionDevice_Crestron_Sample_DoorLock_CloudConnected
{
	public class DoorLockProtocol : ABaseDriverProtocol
	{
		public DoorLockProtocol(ISerialTransport transport, byte id) : base(transport, id)
		{
		}

		protected override void ConnectionChangedEvent(bool connection)
		{
		}

		protected override void ChooseDeconstructMethod(ValidatedRxData validatedData)
		{
		}

		public override void SetUserAttribute(string attributeId, string attributeValue)
		{
		}

		public override void SetUserAttribute(string attributeId, bool attributeValue)
		{
		}

		public override void SetUserAttribute(string attributeId, ushort attributeValue)
		{
		}
	}
}