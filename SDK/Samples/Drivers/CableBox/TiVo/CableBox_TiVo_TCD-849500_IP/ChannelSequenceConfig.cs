// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.RAD.Drivers.CableBoxes
{
    public class ChannelSequenceConfig
    {
        public uint MinimumNumberOfDigits { get; set; }
        public bool TriggerEnterAfterCommands { get; set; }
        public uint DelayBetweenCommands { get; set; }
        public uint DelayBetweenSequences { get; set; }
        public uint IRCommandDuration { get; set; }
    }
}