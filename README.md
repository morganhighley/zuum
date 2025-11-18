# Zuum Media H10X10-4K6G Crestron Home Driver

Complete Crestron Home Extension driver for the Zuum Media H10X10-4K6G 10x10 HDMI Matrix Switcher.

## Device Overview

The Zuum Media H10X10-4K6G is a professional 10x10 HDMI matrix switcher with the following capabilities:

- **Resolution**: Up to 4K@60Hz 4:4:4
- **Inputs**: 10 x HDMI 2.0
- **Outputs**: 10 x HDMI 2.0
- **Audio Extraction**: 2 x S/PDIF coaxial outputs
- **Video Bandwidth**: 18 Gbps
- **HDCP**: 2.2 compliant
- **HDR Support**: HDR10, Dolby Vision, HLG
- **Audio Formats**: LPCM 7.1@192KHz, Dolby TrueHD, DTS-HD MA
- **ARC Support**: Yes (Audio Return Channel)

## Driver Files

1. **zuum-h10x10-4k6g.json** - Main driver configuration file for Crestron Home
2. **zuum-h10x10-control.js** - JavaScript control library for advanced functionality
3. **README.md** - This documentation file

## Installation

1. Copy `zuum-h10x10-4k6g.json` to your Crestron Home drivers directory
2. Copy `zuum-h10x10-control.js` to your Crestron Home scripts directory
3. Add the device in Crestron Home using the driver
4. Configure RS-232 connection settings

## Connection Settings

### RS-232 Configuration
- **Baud Rate**: 9600
- **Data Bits**: 8
- **Stop Bits**: 1
- **Parity**: None
- **Flow Control**: None
- **Delimiter**: CR+LF (0D0A)

### Network Configuration (for Web GUI/TCP control)
- **Default IP**: 192.168.0.10
- **TCP Port**: 47011
- **Web GUI Port**: 80

## Available Commands

### Power Control

| Command | Description |
|---------|-------------|
| Power On | Turn on the matrix |
| Power Off | Put matrix in standby mode |

**Example:**
```javascript
const controller = new ZuumH10X10Controller();
controller.powerOn();  // Returns: "POWER 01\r\n"
controller.powerOff(); // Returns: "POWER 00\r\n"
```

### Input/Output Routing

#### Route Single Input to Single Output

| Parameter | Range | Description |
|-----------|-------|-------------|
| Output | 1-10 | Output number |
| Input | 0-10 | Input number (0 = OFF) |

**Example:**
```javascript
// Route Input 3 to Output 5
controller.routeInputToOutput(5, 3);

// Turn off Output 7
controller.turnOffOutput(7);
```

#### Route Input to Multiple Outputs

**Example:**
```javascript
// Route Input 2 to Outputs 1, 3, and 5
controller.routeInputToMultipleOutputs([1, 3, 5], 2);

// Route Input 1 to all outputs
controller.routeInputToAll(1);
```

#### Route All Outputs

**Example:**
```javascript
// Turn off all outputs
controller.turnOffAllOutputs();

// Mirror Input 4 to all outputs
controller.mirrorInput4ToAll();
```

#### Default 1:1 Routing

**Example:**
```javascript
// Set default routing (Input 1->Output 1, Input 2->Output 2, etc.)
controller.setDefaultRouting();
```

### Audio Extraction Control

The matrix has 2 S/PDIF outputs that can extract audio from either HDMI or ARC sources.

| SPDIF Output | Source Options |
|--------------|----------------|
| 1 | HDMI (H) or ARC (A) |
| 2 | HDMI (H) or ARC (A) |

**Example:**
```javascript
// Set SPDIF 1 to extract HDMI audio
controller.spdif1SelectHDMI();

// Set SPDIF 2 to use ARC audio
controller.spdif2SelectARC();

// Or use the generic method
controller.selectAudioSource(1, 'H'); // SPDIF 1, HDMI
controller.selectAudioSource(2, 'A'); // SPDIF 2, ARC
```

### Scene Management

The matrix supports 10 preset scenes that save complete routing and audio configurations.

**Example:**
```javascript
// Save current configuration to Scene 1
controller.saveScene(1);

// Load Scene 3
controller.loadScene(3);

// Save all scenes
for (let i = 1; i <= 10; i++) {
    controller.saveScene(i);
}
```

### Panel Lock Control

Lock or unlock the front panel buttons and IR remote control.

**Example:**
```javascript
// Lock front panel and IR remote
controller.lockPanel();

// Unlock front panel and IR remote
controller.unlockPanel();

// Or use the generic method
controller.setPanelLock(true);  // Lock
controller.setPanelLock(false); // Unlock
```

### IR Remote ID Control

Set the IR remote ID (0-9) to match multiple matrices or avoid conflicts.

**Example:**
```javascript
// Set IR ID to 5
controller.setIRID(5);
```

### Network Configuration

Configure network settings via RS-232.

**Example:**
```javascript
// Enable DHCP
controller.setDHCP(true);

// Or set static IP configuration
controller.setDHCP(false);
controller.setIPAddress('192.168.1.100');
controller.setSubnetMask('255.255.255.0');
controller.setGateway('192.168.1.1');

// Set network speed
controller.setMediaType('AUTO');  // Auto-detect
controller.setMediaType('100M');  // 100 Mbps
controller.setMediaType('10M');   // 10 Mbps

// Enable MAC address filtering
controller.setMACFilter(true);
```

### Status Queries

**Example:**
```javascript
// Get connection status
controller.getStatus();

// Get firmware version
controller.getVersion();

// Get command help
controller.getHelp();
```

## Custom Routing Patterns

Use the `RoutingPatterns` helper class for zone-based control.

**Example:**
```javascript
const controller = new ZuumH10X10Controller();
const patterns = RoutingPatterns.matrix(controller);

// Route to specific zones
patterns.livingRoom(3);      // Route Input 3 to Living Room (Outputs 1,2)
patterns.bedroom(5);          // Route Input 5 to Bedroom (Output 3)
patterns.theater(2);          // Route Input 2 to Theater (Outputs 6,7,8)

// Source-based routing
patterns.cableBoxToAll();     // Route cable box (Input 1) to all outputs
patterns.blurayToTheater();   // Route Blu-ray (Input 2) to theater
patterns.streamingToLivingRoom(); // Route streaming (Input 3) to living room

// Turn everything off
patterns.allOff();
```

## Custom Routing Configuration

Create complex routing scenarios with custom configurations.

**Example:**
```javascript
// Define custom routing
const customConfig = {
    1: 3,  // Output 1 <- Input 3
    2: 3,  // Output 2 <- Input 3
    3: 5,  // Output 3 <- Input 5
    4: 0,  // Output 4 <- OFF
    5: 2,  // Output 5 <- Input 2
    6: 1,  // Output 6 <- Input 1
    7: 1,  // Output 7 <- Input 1
    8: 1,  // Output 8 <- Input 1
    9: 4,  // Output 9 <- Input 4
    10: 0  // Output 10 <- OFF
};

const commands = controller.createCustomRouting(customConfig);
// Execute all commands
```

## Feedback Parsing

The device responds with "OK" for successful commands and "NG" for errors.

**Example:**
```javascript
const response = "OK";
const parsed = controller.parseResponse(response);
// { success: true, message: 'Command executed successfully' }

const errorResponse = "NG";
const parsedError = controller.parseResponse(errorResponse);
// { success: false, message: 'Command failed or invalid' }
```

## Getting Current State

**Example:**
```javascript
// Get current routing configuration
const routing = controller.getCurrentRouting();
console.log(routing);
// {
//   1: 3,  // Output 1 is showing Input 3
//   2: 5,  // Output 2 is showing Input 5
//   ...
// }

// Get current audio source configuration
const audioSources = controller.getCurrentAudioSources();
console.log(audioSources);
// {
//   1: 'H',  // SPDIF 1 is using HDMI audio
//   2: 'A'   // SPDIF 2 is using ARC audio
// }
```

## Complete Integration Example

```javascript
// Initialize controller
const matrix = new ZuumH10X10Controller();

// Power on
matrix.powerOn();

// Set up routing for a home theater system
const commands = [
    // Living room - Apple TV
    matrix.routeInputToOutput(1, 3),
    matrix.routeInputToOutput(2, 3),

    // Bedroom - Cable box
    matrix.routeInputToOutput(3, 1),

    // Kitchen - Streaming device
    matrix.routeInputToOutput(4, 5),

    // Home theater - Blu-ray player
    matrix.routeInputToOutput(6, 2),
    matrix.routeInputToOutput(7, 2),
    matrix.routeInputToOutput(8, 2),

    // Game room - Gaming console
    matrix.routeInputToOutput(9, 4),
    matrix.routeInputToOutput(10, 4)
];

// Save as Scene 1
matrix.saveScene(1);

// Configure audio extraction
matrix.spdif1SelectHDMI();  // SPDIF 1 extracts HDMI audio
matrix.spdif2SelectARC();   // SPDIF 2 uses ARC from TV

// Load the scene later
matrix.loadScene(1);
```

## Quick Reference Commands

### Power
- `POWER 01` - Power On
- `POWER 00` - Power Off (Standby)

### Routing
- `TX01 05` - Route Input 5 to Output 1
- `TX10 00` - Turn Off Output 10

### Audio
- `AUDIO01 H` - SPDIF 1 select HDMI audio
- `AUDIO02 A` - SPDIF 2 select ARC audio

### Scenes
- `SAVE 01` - Save Scene 1
- `LOAD 05` - Load Scene 5

### Control
- `LOCK 01` - Lock panel
- `LOCK 00` - Unlock panel
- `IRID 04` - Set IR ID to 4

### Network
- `DHCP 01` - Enable DHCP
- `IP_ADDRESS 192.168.0.10` - Set IP
- `SUBNET_MASK 255.255.255.0` - Set subnet
- `GATEWAY 192.168.0.1` - Set gateway
- `MEDIA_TYPE 00` - Auto (00=Auto, 01=10M, 02=100M)
- `MAC_FILTER 01` - Enable MAC filter

### Status
- `STATUS` - Get status
- `VERSION` - Get version
- `HELP` - Get help

## Supported Features

- ✅ Full 10x10 routing control
- ✅ Individual output control
- ✅ Multi-output routing
- ✅ All outputs routing
- ✅ Output on/off control
- ✅ 10 scene presets (save/load)
- ✅ 2 SPDIF audio extraction outputs
- ✅ HDMI and ARC audio source selection
- ✅ Power control
- ✅ Panel lock/unlock
- ✅ IR remote ID configuration
- ✅ Network configuration (DHCP, static IP)
- ✅ Status queries
- ✅ Firmware version query
- ✅ Custom routing patterns
- ✅ Zone-based control
- ✅ Batch command support

## Device Specifications

### Video
- **HDMI Version**: 2.0
- **HDCP**: 2.2
- **Resolution**: Up to 4K@60Hz 4:4:4
- **Bandwidth**: 18 Gbps
- **3D Support**: Yes
- **HDR**: HDR10, Dolby Vision, HLG

### Audio
- **Format Support**: LPCM 7.1@192KHz, Dolby TrueHD, DTS-HD MA
- **Audio Extraction**: 2 x S/PDIF coaxial
- **ARC**: Yes

### Control
- **RS-232**: DB9 Female (Console), 9600 baud
- **Ethernet**: RJ45, TCP/IP control (port 47011)
- **IR**: 3.5mm jack, 45° range, 5m distance
- **USB**: Type-A for firmware updates
- **Front Panel**: LCD + buttons
- **Web GUI**: Yes (HTTP port 80)
- **Mobile App**: Android/iOS

### Physical
- **Dimensions**: 19" x 9.13" x 1.73" (483 x 232 x 44mm)
- **Weight**: 6.44 lbs (2.92 kg)
- **Rack**: 1U standard
- **Power**: DC 12V 6.67A

### Environmental
- **Operating Temp**: 32°F to 131°F (0°C to 55°C)
- **Storage Temp**: -4°F to 185°F (-20°C to 85°C)
- **Humidity**: Up to 95%

## Troubleshooting

### Command Not Working

1. Verify RS-232 connection settings (9600, 8, N, 1)
2. Ensure proper command format with space between command and parameter
3. Check that commands end with CR+LF (\\r\\n)
4. Commands are case-insensitive
5. Device responds with "OK" or "NG"

### Panel Locked

- Hold MENU button for 10 seconds to unlock

### IR Remote Not Working

1. Check IR ID matches matrix (default is 4)
2. Set IR ID on remote: Hold POWER + number key
3. In standby mode, IR only powers on the matrix

### Network Issues

1. Default IP: 192.168.0.10
2. Default Subnet: 255.255.255.0
3. Use front panel or RS-232 to reconfigure
4. Maximum 4 simultaneous web connections

### Firmware Update

1. Copy firmware to USB drive root directory
2. Insert USB drive
3. Front panel: Select USB UPDATE, press ENTER
4. OR: Power off, hold SET button, power on
5. Wait for update to complete (do not power off)

## Support

For additional support:
- Refer to the H10X10-4K6G Manual.pdf included in repository
- Check Zuum Media website for firmware updates
- Contact Zuum Media technical support

## Version History

### Version 1.0.0 (Initial Release)
- Complete 10x10 routing control
- Scene management (10 presets)
- Audio extraction control (2 S/PDIF outputs)
- Power control
- Panel lock control
- IR ID configuration
- Network configuration
- Status queries
- Custom routing patterns
- Zone-based control
- Batch command support
- Comprehensive documentation

## License

This driver is provided for use with Crestron Home systems controlling the Zuum Media H10X10-4K6G HDMI Matrix Switcher.

## Author

Created for Crestron Home integration

---

**Note**: Commands must be sent with proper delimiter (CR+LF). All commands are case-insensitive. Device responds with "OK" for success or "NG" for failure.
