# Visual Studio Compatibility Guide for Crestron Drivers

## The Issue

You're seeing this error when trying to open the .csproj files in Visual Studio 2022:

```
Unsupported
This version of Visual Studio is unable to open the following projects.
The project types may not be installed or this version of Visual Studio may not support them.
```

**Root Cause:** Crestron drivers for embedded processors (including Crestron Home 4-Series) use **.NET Compact Framework 3.5** targeting **Windows CE**. This project type was deprecated and **removed from Visual Studio after VS 2013**.

## Why This Format?

- Crestron's embedded processors run a custom Windows CE-based operating system
- The Rapid Application Development (RAD) Framework requires .NET Compact Framework
- All official Crestron SDK sample drivers use this same format
- The project files include these specific GUIDs that VS 2022 doesn't recognize:
  - `{0B4745B0-194B-4BB6-8E21-E9057CA92500}` - Smart Device Project
  - `{4D628B5B-2FBC-4AA6-8C16-197242AEB884}` - Windows CE
  - `{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}` - C#

## Solutions

### Solution 1: Install Visual Studio 2008 (Officially Supported)

The Crestron SDK samples explicitly target **Visual Studio 2008**, which has full support for Smart Device projects.

**Steps:**
1. Download Visual Studio 2008 Professional (or find your existing installation media)
2. Install alongside your existing VS 2022 (they can coexist)
3. Open the .csproj files in VS 2008
4. Build and package normally

**Pros:**
- Officially supported by Crestron
- Full IDE experience (IntelliSense, debugging, etc.)
- No compatibility issues

**Cons:**
- VS 2008 is old and may be hard to obtain
- Limited modern C# language features

### Solution 2: Use Visual Studio 2013 with Update 5

VS 2013 was the **last version** to support .NET Compact Framework.

**Steps:**
1. Install Visual Studio 2013 Update 5
2. Install ".NET Compact Framework" tooling from VS installer
3. Open and build projects normally

**Pros:**
- More modern than VS 2008
- Still supports Compact Framework
- Easier to find than VS 2008

**Cons:**
- Still an older IDE
- May require special licensing

### Solution 3: Build from Command Line (Recommended Workaround)

You can build the drivers using **MSBuild** directly without opening them in Visual Studio IDE.

**Steps:**
1. Use the provided `build_drivers.cmd` script
2. Run from Windows Command Prompt:
   ```cmd
   build_drivers.cmd
   ```

The script will:
- Locate MSBuild (from your VS 2022 installation)
- Build both Serial and IP drivers
- Package them using ManifestUtil
- Create .pkg files ready for deployment

**Pros:**
- Works with your existing VS 2022 installation
- No need to install old Visual Studio versions
- Automated build process
- Can be integrated into CI/CD

**Cons:**
- No IDE experience for editing/debugging
- Must edit .cs files in VS Code or another editor
- Cannot use Visual Studio's project management features

**Alternative: Edit in VS Code + Build with Script**
- Edit C# files in Visual Studio Code (with C# extension)
- Use the build script to compile
- Best of both worlds for many developers

### Solution 4: Contact Crestron for Updated SDK

Crestron may have updated build tools or project formats for modern Visual Studio.

**Steps:**
1. Contact Crestron Developer Support
2. Ask if there's a VSIX extension for VS 2022
3. Check if newer SDK versions support modern project formats

## Recommended Approach

For immediate development:
1. **Edit code** in Visual Studio Code with C# extension
2. **Build** using the `build_drivers.cmd` script
3. **Test** on actual Crestron Home processors or with XPanel

For long-term development:
1. Install **Visual Studio 2013 Update 5** for full IDE support
2. Use for both editing and building
3. Keep VS 2022 for other modern .NET projects

## Build Output Locations

After building (via any method), your driver packages will be at:

- **Serial Driver:** `MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial/bin/Release/MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.pkg`
- **IP Driver:** `MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP/bin/Release/MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP.pkg`

## Testing the Build Script

To verify the command-line build works:

```cmd
cd C:\Users\Premier Visions\Desktop\zuum
build_drivers.cmd
```

If successful, you'll see:
```
Build Complete!
Output files:
  - MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial\bin\Release\MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.pkg
  - MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP\bin\Release\MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP.pkg
```

## Common Build Issues

### Issue: "MSBuild not found"
**Solution:** Update the `MSBUILD_PATH` in `build_drivers.cmd` to match your VS installation path.

### Issue: "SDK DLLs not found"
**Solution:** Ensure the `SDK/Libraries/` folder contains:
- RADCommon.dll
- RADCableBox.dll
- RADProTransports.dll
- Crestron.DeviceDrivers.API.dll

### Issue: "SimplSharp DLLs not found"
**Solution:** Ensure Crestron SDK is installed at:
- `C:\ProgramData\Crestron\SDK\`

## Project File Structure

Both drivers use this structure:
```
MatrixSwitcher_ZuumMedia_H10X10-4K6G_[Serial|IP]/
├── *.csproj                          # VS 2008 format project file
├── H10X10MatrixSwitcher*.cs          # Main driver class
├── H10X10MatrixSwitcherProtocol.cs   # Protocol handler (shared)
├── H10X10ResponseValidator.cs        # Response validator (shared)
├── H10X10MatrixSwitcher*.json        # JSON manifest (embedded resource)
└── Properties/
    └── AssemblyInfo.cs               # Assembly metadata
```

## Additional Resources

- **Crestron Developer Network:** https://developer.crestron.com
- **SDK Documentation:** `SDK/Documentation/SDKDocs/`
- **Sample Drivers:** `SDK/Samples/Drivers/`
- **Driver Best Practices:** Check the HTML files provided in the repository

## Summary

The .csproj files are **correctly formatted** for Crestron driver development. The issue is purely a Visual Studio version compatibility problem. Use the build script as a workaround, or install VS 2008/2013 for full IDE support.
