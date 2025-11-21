# Visual Studio 2019/2022 Compatibility for Crestron Home Drivers

## Overview

The Zuum H10X10-4K6G Matrix Switcher drivers have been configured for **Visual Studio 2019 or later** using the modern **.NET Framework Class Library** project format, as required for **Crestron Home and 4-Series processors**.

## Project Format

According to [official Crestron SDK documentation](https://www.crestron.com):

> "If your development target is for a residential application or includes 4-Series™ control systems, you can use Visual Studio® 2019 or Visual Studio 2008 software."

### VS 2019+ Requirements

The project files use:
- **Project Type:** .NET Framework Class Library
- **Framework Version:** .NET Framework 4.7.2
- **Build Tools:** MSBuild (standard .NET build process)
- **Required Workload:** .NET desktop development

## Setup Instructions

### 1. Install Visual Studio 2019 or 2022

1. Download from: https://visualstudio.microsoft.com/downloads/
2. During installation, select the **".NET desktop development"** workload
3. Complete the installation

### 2. Install Crestron SDK

Ensure the Crestron SDK is installed at:
- `C:\ProgramData\Crestron\SDK\`

This provides required DLLs:
- SimplSharpCustomAttributesInterface.dll
- SimplSharpHelperInterface.dll
- SimplSharpReflectionInterface.dll

### 3. Verify SDK Libraries

Ensure the following files exist in `SDK/Libraries/`:
- RADCommon.dll
- RADCableBox.dll
- RADProTransports.dll
- Crestron.DeviceDrivers.API.dll

## Opening the Projects

### In Visual Studio 2019/2022

1. Open Visual Studio
2. File → Open → Project/Solution
3. Navigate to the project folder and select the `.csproj` file:
   - `MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.csproj` OR
   - `MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP.csproj`
4. The project should load successfully

### Expected Behavior

✅ **Should work:** Visual Studio 2019, 2022
✅ **Target Platform:** Crestron Home, 4-Series processors
✅ **Build Output:** .NET Framework 4.7.2 DLL
✅ **Packaging:** ManifestUtil creates .pkg files

## Building the Drivers

### Option 1: Build in Visual Studio (Recommended)

1. Open the .csproj file in Visual Studio
2. Select **Release** configuration
3. Build → Build Solution (or press Ctrl+Shift+B)
4. Output will be in `bin\Release\`

### Option 2: Build from Command Line

Use the provided `build_drivers.cmd` script:

```cmd
cd C:\Users\Premier Visions\Desktop\zuum
build_drivers.cmd
```

The script will:
- Build both Serial and IP drivers
- Package them using ManifestUtil
- Create .pkg files for deployment

### Option 3: MSBuild Directly

```cmd
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" ^
  MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial\MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.csproj ^
  /p:Configuration=Release /t:Rebuild
```

## Project Structure

Both drivers use modern .NET Framework project format:

```xml
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
    <OutputType>Library</OutputType>
  </PropertyGroup>
  <!-- References to Crestron SDK libraries -->
  <!-- Source files -->
</Project>
```

Key differences from older VS 2008 format:
- ❌ No Windows CE ProjectTypeGuids
- ❌ No Compact Framework
- ✅ Standard .NET Framework 4.7.2
- ✅ Modern MSBuild targets
- ✅ Compatible with VS 2019/2022

## Packaging Drivers

After building, package the DLLs using ManifestUtil:

### Serial Driver
```cmd
SDK\ManifestUtil\ManifestUtil.exe ^
  MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial\bin\Release\MatrixSwitcher_ZuumMedia_H10X10-4K6G_Serial.dll
```

### IP Driver
```cmd
SDK\ManifestUtil\ManifestUtil.exe ^
  MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP\bin\Release\MatrixSwitcher_ZuumMedia_H10X10-4K6G_IP.dll
```

Output .pkg files will be created in the same directory.

## Deployment

The packaged .pkg files can be deployed to:
- **Crestron Home** (residential applications)
- **4-Series processors** (MC4, CP4, CP4N, etc.)

### Deployment Methods

1. **Crestron Home App:**
   - Upload .pkg files through Crestron Home configuration
   - Add device to the system

2. **Crestron Toolbox:**
   - Connect to the processor
   - Upload driver .pkg files
   - Configure device in SIMPL or Crestron Home

## Troubleshooting

### Issue: "Could not find SDK DLLs"

**Solution:** Verify that:
1. SDK folder exists at the correct location
2. All required DLLs are present in `SDK/Libraries/`
3. SimplSharp DLLs exist in `C:\ProgramData\Crestron\SDK\`

### Issue: "Project failed to load"

**Solution:**
1. Ensure you have the ".NET desktop development" workload installed
2. Restart Visual Studio
3. Try opening the project again

### Issue: "ManifestUtil fails"

**Solution:**
1. Ensure the DLL was built successfully
2. Check that the embedded JSON manifest file is present
3. Verify ManifestUtil.exe is in `SDK/ManifestUtil/`

## NuGet Packages (Optional)

For enhanced compatibility, you can also add Crestron NuGet packages:
- Crestron.SimplSharp.SDK.Library
- Crestron.SimplSharp.SDK.ProgramLibrary

These are optional as the local DLL references should work.

## Version Compatibility

| Visual Studio | .NET Framework | Crestron Target | Status |
|--------------|----------------|-----------------|--------|
| VS 2022      | 4.7.2+         | 4-Series/Home  | ✅ Supported |
| VS 2019      | 4.7.2+         | 4-Series/Home  | ✅ Supported |
| VS 2017      | 4.7.2+         | 4-Series/Home  | ⚠️ May work |
| VS 2013      | 3.5 CF         | 3-Series       | Legacy only |
| VS 2008      | 3.5 CF         | 3-Series       | Legacy only |

**Note:** VS 2008 format (Compact Framework) is for 3-Series **commercial** applications only. For Crestron Home and 4-Series residential applications, use VS 2019+ with .NET Framework 4.7.2.

## Official Crestron Documentation

For more information, refer to the official Crestron SDK documentation:
- **System Requirements:** `SDK/Documentation/DeveloperWebsite/.../System-Requirements.htm`
- **Installation and Setup:** `SDK/Documentation/DeveloperWebsite/.../Install-Setup.htm`
- **Create a Project:** `SDK/Documentation/DeveloperWebsite/.../Create-a-Project.htm`

## Summary

✅ **Projects are now configured for VS 2019/2022**
✅ **Modern .NET Framework 4.7.2 format**
✅ **Compatible with Crestron Home and 4-Series**
✅ **Can be opened and built in Visual Studio 2019, 2022, or via command line**

The drivers should now open successfully in your Visual Studio 2022 installation!
