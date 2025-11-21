# Zuum Media H10X10-4K6G Matrix Switcher Drivers for Crestron Home

This repository contains **two Crestron Home device drivers** for the Zuum Media H10X10-4K6G 10x10 HDMI Matrix Switcher:

1. **RS-232 Serial Driver** - Control via serial port
2. **TCP/IP Driver** - Control via Ethernet network

## Device Overview

The H10X10-4K6G is a professional 10x10 HDMI matrix switcher with multiple control options.

**Device Specifications:**
- 10 HDMI inputs, 10 HDMI outputs
- 4K@60Hz (4:4:4) support
- HDCP 2.2, HDR10, Dolby Vision, HLG
- 18 Gbps video bandwidth
- LPCM 7.1, Dolby TrueHD, DTS-HD MA audio support
- Multiple control methods: RS-232, TCP/IP, Web GUI, IR, Mobile Apps

**Driver Version:** 6.00.000.0026
**SDK Version:** 6.00.000.0025+

## Available Drivers

### 1. RS-232 Serial Driver
**Directory:** `MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial/`

**Communication Settings:**
- Baud Rate: 9600
- Data Bits: 8
- Parity: None
- Stop Bits: 1
- Delimiter: CR+LF (`\r\n`)

**Use When:**
- Direct serial connection is required
- RS-232 cable is already in place
- Network control is not available or desired
- Maximum reliability with physical connection

### 2. TCP/IP Network Driver
**Directory:** `MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP/`

**Communication Settings:**
- Protocol: TCP
- Default Port: 47011
- Default IP: 192.168.0.10 (configurable on device)
- Delimiter: CR+LF (`\r\n`)

**Use When:**
- Network-based control is preferred
- Device is connected to the same network as Crestron processor
- Remote control over IP network is required
- Multiple controllers need access to the device

## Command Protocol

Both drivers use the **same command protocol** with identical commands:

| Command | Format | Description | Response |
|---------|--------|-------------|----------|
| Power On | `POWER 01` | Turn matrix power on | `OK` or `NG` |
| Power Off | `POWER 00` | Turn matrix power off | `OK` or `NG` |
| Route | `TX[output] [input]` | Route input to output | `OK` or `NG` |
| Save Scene | `SAVE [scene]` | Save current routing (1-8) | `OK` or `NG` |
| Load Scene | `LOAD [scene]` | Load saved routing (1-8) | `OK` or `NG` |

**Example:** `TX5 3` routes Input 3 to Output 5

All commands are terminated with CR+LF (`\r\n`).
Responses: `OK` for success, `NG` for failure.

## Choosing the Right Driver

| Factor | Serial Driver | TCP/IP Driver |
|--------|--------------|--------------|
| **Connection** | Direct RS-232 cable | Ethernet network |
| **Distance** | Limited by serial cable (50ft typical) | Unlimited over network |
| **Reliability** | Very high (physical connection) | High (depends on network) |
| **Multiple Controllers** | Single controller only | Multiple controllers possible |
| **Installation** | Requires serial port on processor | Requires network connectivity |
| **Latency** | Very low | Low (network dependent) |

**Recommendation:** Use **TCP/IP driver** for most modern installations where network infrastructure is available. Use **Serial driver** for legacy systems or when maximum reliability is critical.

## Prerequisites

### Required Software

1. **Visual Studio**
   - **Visual Studio 2019** (Recommended for Crestron Home/4-Series)
     - Download: https://visualstudio.microsoft.com/downloads/
     - Select ".NET desktop development" workload during installation
   - **Visual Studio 2008** (For 3-Series compatibility)
     - Download: https://www.microsoft.com/en-us/download/details.aspx?id=7873

2. **Crestron Drivers SDK v6.00.000.0025+**
   - Located in the `/SDK` directory of this repository
   - Contains required libraries and tools (RADCommon.dll, RADCableBox.dll, etc.)

3. **Additional Tools** (Optional but recommended)
   - Visual Studio Code with JSON extension (for editing JSON manifests)
   - Crestron Toolbox (for deployment and testing)
   - Crestron Database and Device Database (latest versions)

### Required Hardware for Testing

- **For Crestron Home:** Crestron 4-Series Control Processor (CP4-R, DIN-AP4-R, or MC4-R)
- **For Commercial:** Crestron 3-Series Control System
- **Device:** Zuum Media H10X10-4K6G Matrix Switcher
- **For Serial Driver:** RS-232 serial cable (straight-through)
- **For TCP/IP Driver:** Ethernet cable (CAT5e or better) and network switch/router

## Quick Start - Building the Drivers

### Step 1: Choose Your Driver

Navigate to the appropriate driver directory:
- **Serial:** `cd MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial`
- **TCP/IP:** `cd MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP`

### Step 2: Open in Visual Studio

Double-click the `.csproj` file to open in Visual Studio:
- `MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.csproj`
- `MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP.csproj`

### Step 3: Build the Project

- Select **Build > Build Solution** (or press Ctrl+Shift+B)
- Check the Output window for: `========== Build: 1 succeeded ==========`
- Find the compiled DLL in `bin\Release\` or `bin\Debug\`

### Step 4: Create Deployment Package

Use ManifestUtil to create the `.pkg` file:

```bash
cd bin\Release
..\..\..\..\SDK\ManifestUtil\ManifestUtil.exe
```

This creates the `.pkg` file for deployment to Crestron Home.

## Driver Architecture

Both drivers follow the Crestron RAD Framework V1 architecture:

```
Driver Layer
    ↓ implements ITcp or ISerialComport
    ↓ inherits ACableBox
Protocol Layer
    ↓ inherits ACableBoxProtocol
    ↓ handles command formatting
Response Validator
    ↓ inherits ResponseValidation
    ↓ validates responses
Transport Layer
    ↓ TcpTransport or CommonSerialComport
    ↓ handles physical communication
```

### Project Files

Both driver projects contain:
- **JSON Manifest** - Device capabilities, commands, and communication settings
- **Driver Class** - Main driver implementing ITcp or ISerialComport
- **Protocol Class** - Command formatting and response parsing (shared between both)
- **Response Validator** - Response validation logic (shared between both)
- **AssemblyInfo.cs** - Assembly metadata
- **Project File (.csproj)** - Visual Studio project configuration

## Deployment

### To Crestron Home (4-Series)

1. Build the driver in **Release** configuration
2. Create `.pkg` file using ManifestUtil
3. Open Crestron Toolbox and connect to your processor
4. Navigate to **Device Management > Drivers**
5. Upload the `.pkg` file
6. Add device in Crestron Home interface
7. Configure connection settings (Serial: baud rate, port / IP: address, port)

### Configuration Settings

**Serial Driver:**
- COM Port: Select available serial port
- Baud Rate: 9600
- Data Bits: 8
- Parity: None
- Stop Bits: 1

**TCP/IP Driver:**
- IP Address: Device IP (default: 192.168.0.10)
- Port: 47011
- Auto Reconnect: Enabled

## Testing

### Serial Driver Testing
1. Connect matrix switcher to processor via RS-232 cable (straight-through)
2. Verify cable pinout: TX→RX, RX→TX, GND→GND
3. Power on matrix switcher
4. Test power on/off commands
5. Test input routing (e.g., Input 1 → Output 1)
6. Verify LED feedback on front panel
7. Check Crestron Home UI for status updates

### TCP/IP Driver Testing
1. Connect matrix switcher to network
2. Configure matrix IP address (default: 192.168.0.10)
3. Verify network connectivity: `ping 192.168.0.10`
4. Test Telnet connection: `telnet 192.168.0.10 47011`
5. Send test command: `POWER 01` (should respond `OK`)
6. Configure driver in Crestron Home with device IP
7. Test all routing commands
8. Verify feedback and status updates

## Troubleshooting

### Serial Driver Issues

**Problem:** No response from device
**Solutions:**
- Verify serial cable is straight-through (not null-modem)
- Check baud rate is set to 9600
- Confirm COM port assignment in Crestron processor
- Test cable with another terminal program (PuTTY, etc.)
- Verify matrix switcher power and RS-232 settings

**Problem:** Garbled responses
**Solutions:**
- Verify baud rate matches (9600)
- Check data bits (8), parity (None), stop bits (1)
- Replace serial cable if damaged

### TCP/IP Driver Issues

**Problem:** Cannot connect to device
**Solutions:**
- Verify device IP address configuration
- Ping device IP to test network connectivity
- Check firewall settings (port 47011 must be open)
- Verify processor and matrix are on same subnet
- Check network cable and switch connections

**Problem:** Connection drops randomly
**Solutions:**
- Enable Auto Reconnect in driver settings
- Check network switch for errors/collisions
- Verify DHCP lease is not expiring (use static IP)
- Update network switch firmware
- Check for network congestion

### General Issues

**Problem:** Commands not working
**Solutions:**
- Enable driver logging in Crestron Home
- View CrestronConsole for TX/RX debug output
- Verify command syntax matches device manual
- Check delimiter is CR+LF (`\r\n`)
- Test commands manually via Telnet (IP) or terminal (Serial)

**Problem:** Build errors in Visual Studio
**Solutions:**
- Verify SDK libraries exist in `../SDK/Libraries/`
- Check SimplSharp DLLs path: `C:\ProgramData\Crestron\SDK\`
- Ensure .NET Framework 3.5 is installed
- Clean and rebuild solution
- Check for typos in `.csproj` file paths

## Advanced Configuration

### Changing Network Settings (TCP/IP Driver)

To change the matrix switcher IP address:

**Via Front Panel:**
1. Press MENU button
2. Navigate to ETHERNET
3. Select IP ADDRESS
4. Use arrow keys to modify
5. Press ENTER to save

**Via Web GUI:**
1. Browse to current IP (default: http://192.168.0.10)
2. Click Advanced
3. Modify IP Address, Subnet Mask, Gateway
4. Click Apply
5. Power cycle matrix switcher

**Via Command (Telnet/Serial):**
```
IP_ADDRESS 192.168.1.100
SUBNET_MASK 255.255.255.0
GATEWAY 192.168.1.1
```

### Scene Management

Both drivers support 8 programmable scenes:

**Save Current Routing:**
```
SAVE 1    // Save to scene 1
SAVE 2    // Save to scene 2
...
SAVE 8    // Save to scene 8
```

**Load Saved Routing:**
```
LOAD 1    // Load scene 1
LOAD 2    // Load scene 2
...
LOAD 8    // Load scene 8
```

Scenes remember all 10 input-to-output routing assignments.

## Support Resources

- **Device Manual:** `H10X10-4K6G Manual.pdf` (in repository root)
- **SDK Documentation:** `SDK/Documentation/`
- **Crestron RAD Framework Guide:** See SDK documentation
- **Best Practices:** Review `Best Practices _ Crestron® Drivers Developer Microsite.html`
- **Installation Guide:** Review `Installation and Setup _ Crestron® Drivers Developer Microsite.html`

## Version History

- **v6.00.000.0026** (2025) - Initial release
  - RS-232 serial control support
  - TCP/IP network control support
  - Power control (on/off)
  - 10x10 HDMI matrix routing
  - Scene management (8 scenes)
  - Crestron RAD Framework V1 architecture
  - Full ACableBox device type implementation

## License

Copyright © 2025 Zuum Media / Crestron

Developed for use with Crestron control systems and Crestron Home.

## Additional Notes

- Both drivers use the ACableBox base class since Crestron SDK doesn't have a dedicated Matrix Switcher device type
- Command protocol is identical for serial and TCP/IP - only transport layer differs
- Protocol and response validation classes are shared between both drivers
- Drivers follow all Crestron best practices and naming conventions
- Compatible with both Crestron Home (4-Series) and commercial 3-Series systems
- Supports firmware updates via USB (see device manual for procedure)
- Web GUI accessible at device IP address for additional configuration
- Mobile apps available for iOS and Android (Matrix Controller)
