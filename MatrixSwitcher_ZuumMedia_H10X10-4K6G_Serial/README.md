# Zuum Media H10X10-4K6G Matrix Switcher Driver for Crestron Home

This is a Crestron Home device driver for the Zuum Media H10X10-4K6G 10x10 HDMI Matrix Switcher with RS-232 serial control.

## Overview

The H10X10-4K6G is a 10x10 HDMI matrix switcher supporting 4K@60Hz (4:4:4), HDCP 2.2, and HDR10. This driver enables control of the matrix switcher through Crestron Home via RS-232 serial communication.

**Device Specifications:**
- 10 HDMI inputs, 10 HDMI outputs
- 4K@60Hz (4:4:4) support
- HDCP 2.2 and HDR10 compatible
- RS-232 control at 9600 baud, 8-N-1
- Command/response protocol with CR+LF delimiter

**Driver Version:** 6.00.000.0026
**SDK Version:** 6.00.000.0025+

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
   - Contains required libraries and tools

3. **Additional Tools** (Optional but recommended)
   - Visual Studio Code with JSON extension (for editing JSON manifest)
   - Crestron Toolbox (for deployment and testing)
   - Crestron Database and Device Database (latest versions)

### Required Hardware for Testing

- Crestron 4-Series Control Processor (CP4-R, DIN-AP4-R, or MC4-R) for Crestron Home
- OR Crestron 3-Series Control System for XPanel testing
- Zuum Media H10X10-4K6G Matrix Switcher
- RS-232 serial cable
- Ethernet cable (CAT5e or better)

## Project Structure

```
MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial/
├── H10X10MatrixSwitcher.json              # Device manifest (embedded resource)
├── H10X10MatrixSwitcherSerial.cs          # Main driver class
├── H10X10MatrixSwitcherProtocol.cs        # Protocol handler
├── H10X10ResponseValidator.cs             # Response validator
├── Properties/
│   └── AssemblyInfo.cs                    # Assembly metadata
├── MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.csproj  # VS project file
└── README.md                              # This file
```

## Building the Driver

### Step 1: Open the Project

1. Navigate to the `MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial` directory
2. Double-click `MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.csproj` to open in Visual Studio
3. Visual Studio will load the project with all source files and references

### Step 2: Verify References

The project references the following libraries (located in `../SDK/Libraries/`):
- `RADCommon.dll` - Core RAD framework
- `RADCableBox.dll` - CableBox device type base classes
- `RADProTransports.dll` - Transport layer (serial/IP)
- `Crestron.DeviceDrivers.API.dll` - Device driver API

Additionally, the following system libraries are referenced:
- `SimplSharpCustomAttributesInterface.dll`
- `SimplSharpHelperInterface.dll`
- `SimplSharpReflectionInterface.dll`

**Note:** If you see yellow warning icons on references, verify that:
- The SDK libraries exist in `../SDK/Libraries/`
- SimplSharp DLLs are installed at `C:\ProgramData\Crestron\SDK\`

### Step 3: Build Configuration

1. In Visual Studio, select **Build > Configuration Manager**
2. Choose your build configuration:
   - **Debug**: For development and testing (includes debug symbols)
   - **Release**: For production deployment (optimized)
3. Platform should be set to **AnyCPU**

### Step 4: Build the Project

**Option A: Build via Menu**
1. Select **Build > Build Solution** (or press Ctrl+Shift+B)
2. Check the **Output** window for build results
3. Successful build will show: `========== Build: 1 succeeded, 0 failed ==========`

**Option B: Build via Command Line**
```bash
# Navigate to project directory
cd MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial

# Build using MSBuild (adjust path to your MSBuild.exe)
"C:\Windows\Microsoft.NET\Framework\v3.5\MSBuild.exe" MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.csproj /p:Configuration=Release
```

### Step 5: Verify Build Output

After successful build, verify the output:

**Debug Build:**
- Location: `bin\Debug\`
- Files:
  - `MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.dll` (driver assembly)
  - `MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.pdb` (debug symbols)
  - `RADCommon.dll`, `RADCableBox.dll`, `RADProTransports.dll` (copied dependencies)

**Release Build:**
- Location: `bin\Release\`
- Files:
  - `MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.dll` (driver assembly, optimized)
  - `RADCommon.dll`, `RADCableBox.dll`, `RADProTransports.dll` (copied dependencies)

## Creating the Driver Package (.pkg)

Crestron drivers are deployed as `.pkg` files created using the ManifestUtil tool.

### Step 1: Locate ManifestUtil

ManifestUtil is located in the SDK:
```
../SDK/ManifestUtil/ManifestUtil.exe
```

### Step 2: Run ManifestUtil

**Method 1: From DLL Directory**
```bash
# Navigate to the build output directory
cd bin\Release

# Run ManifestUtil from that location
..\..\..\..\SDK\ManifestUtil\ManifestUtil.exe
```

**Method 2: With Path Argument**
```bash
# Run ManifestUtil with path to DLL
..\SDK\ManifestUtil\ManifestUtil.exe "MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial\bin\Release\MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.dll"
```

**Method 3: Copy ManifestUtil**
```bash
# Copy ManifestUtil.exe to the output directory
copy ..\SDK\ManifestUtil\ManifestUtil.exe bin\Release\
cd bin\Release
ManifestUtil.exe
```

### Step 3: Verify Package Creation

ManifestUtil will create:
- `MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.pkg` - Deployment package

The `.pkg` file contains:
- The compiled DLL
- The embedded JSON manifest
- All required metadata

## Testing the Driver

### Testing with Crestron Home

1. **Load the Package**
   - Use Crestron Toolbox to connect to your 4-Series processor
   - Navigate to **Device Management > Drivers**
   - Upload the `.pkg` file

2. **Add the Device**
   - In Crestron Home, add a new device
   - Search for "Zuum Media H10X10-4K6G"
   - Configure the serial port connection:
     - Baud Rate: 9600
     - Data Bits: 8
     - Parity: None
     - Stop Bits: 1

3. **Test Commands**
   - Power On/Off
   - Route inputs to outputs (e.g., Input 1 → Output 1)
   - Verify feedback in the Crestron Home interface

### Testing with XPanel

1. Load the XPanel test tool (provided in SDK)
2. Configure serial connection to match device settings
3. Test standard commands:
   - Power control
   - Input routing
   - Verify command/response in logs

## Deployment

### Production Deployment Checklist

- [ ] Build in **Release** configuration
- [ ] Test all commands with actual hardware
- [ ] Verify feedback is received correctly
- [ ] Test power on/off cycles
- [ ] Test all input/output routing combinations
- [ ] Document any device-specific requirements
- [ ] Package with ManifestUtil
- [ ] Test `.pkg` file on target system

### Distribution

The `.pkg` file can be distributed to:
- Crestron dealers/programmers
- End users with Crestron Home systems
- Uploaded to private or public driver repositories

## Troubleshooting

### Build Errors

**Error: Cannot find RADCommon.dll or other SDK libraries**
- Verify SDK is located at `../SDK/Libraries/`
- Check that library paths in `.csproj` are correct
- Ensure you extracted the full SDK package

**Error: Cannot find SimplSharp DLLs**
- Install the Crestron SDK to `C:\ProgramData\Crestron\SDK\`
- Or update `.csproj` to point to correct installation location

**Error: JSON manifest not found**
- Verify `H10X10MatrixSwitcher.json` exists in project directory
- Ensure it's marked as **Embedded Resource** in project properties

### Runtime/Testing Errors

**Driver doesn't load in Crestron Home**
- Verify `.pkg` was created successfully with ManifestUtil
- Check Crestron Home logs for error messages
- Ensure processor firmware is up to date

**No response from device**
- Verify serial cable connection
- Confirm baud rate is 9600, 8-N-1
- Check device manual for serial port settings
- Enable driver logging to see TX/RX data

**Commands not working**
- Review device manual for correct command syntax
- Check `H10X10MatrixSwitcher.json` command definitions
- Enable CrestronConsole logging for debug output
- Verify command delimiters (CR+LF)

### Debugging Tips

**Enable Logging:**
```csharp
// In your driver initialization
EnableLogging = true;
EnableRxDebug = true;
EnableTxDebug = true;
```

**View Console Output:**
- Use Crestron Toolbox Console to monitor:
  - Command preparation: `H10X10MatrixSwitcherProtocol_PrepareStringThenSend`
  - Data handling: `H10X10MatrixSwitcherProtocol_DataHandler`
  - Response validation: `H10X10ResponseValidator_ValidateResponse`

**Common Issues:**
- **No ACK/NAK responses**: Check delimiter format (must be `\r\n`)
- **Partial responses**: Verify response validator waits for complete messages
- **Command timeout**: Increase timeout in JSON or verify device is powered on
- **JSON validation errors**: Use JSON validator (VSCode extension) to check syntax

## Driver Architecture

This driver follows the Crestron RAD (Rapid Application Development) Framework V1 architecture:

```
Driver Layer (H10X10MatrixSwitcherSerial)
    ↓ implements ISerialComport, ISimpl
    ↓ inherits ACableBox
    ↓
Protocol Layer (H10X10MatrixSwitcherProtocol)
    ↓ inherits ACableBoxProtocol
    ↓ handles command formatting and data parsing
    ↓
Response Validation (H10X10ResponseValidator)
    ↓ inherits ResponseValidation
    ↓ validates and parses responses
    ↓
Transport Layer (CommonSerialComport)
    ↓ implements ISerialTransport
    ↓ handles physical serial communication
```

### Key Components

- **Driver Class**: Exposes device control to Crestron Home
- **Protocol Class**: Formats commands (adds delimiters) and parses responses
- **Response Validator**: Validates complete messages and extracts data
- **JSON Manifest**: Defines commands, parameters, and device metadata

## Device Command Reference

### Power Commands
- **Power On**: `POWER 01`
- **Power Off**: `POWER 00`

### Routing Commands
- **Route Input to Output**: `TX[output] [input]`
  - Example: `TX1 5` routes Input 5 to Output 1
  - Output range: 1-10
  - Input range: 1-10

### Scene Commands
- **Save Scene**: `SAVE [scene]` (scene range: 1-8)
- **Load Scene**: `LOAD [scene]` (scene range: 1-8)

### Response Format
- **Success**: `OK\r\n`
- **Failure**: `NG\r\n`

## Support

For issues or questions:
1. Review device manual: `../H10X10-4K6G Manual.pdf`
2. Check SDK documentation: `../SDK/Documentation/`
3. Enable debug logging and review console output
4. Contact Zuum Media or Crestron technical support

## Version History

- **v6.00.000.0026** (2025) - Initial release
  - RS-232 serial control support
  - Power control
  - Input/output routing (10x10 matrix)
  - Scene save/load

## License

Copyright © 2025 Zuum Media / Crestron

Developed for use with Crestron control systems and Crestron Home.
