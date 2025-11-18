# Zuum H10X10-4K6G Command Reference

Quick reference guide for RS-232 commands.

## Command Format

All commands follow this format:
```
COMMAND PARAMETER\r\n
```

- Space character (△) required between command and parameter
- Commands are case-insensitive
- Terminator: CR+LF (0D0A hex, \r\n)
- Device responds: "OK" (success) or "NG" (failure)

## Power Control

| Command | Parameter | Description | Example |
|---------|-----------|-------------|---------|
| POWER | 00 | Standby | `POWER 00\r\n` |
| POWER | 01 | Power On | `POWER 01\r\n` |

## Video Routing

| Command | Parameter | Description | Example |
|---------|-----------|-------------|---------|
| TX01 | 00-10 | Route to Output 1 | `TX01 05\r\n` (Input 5 → Output 1) |
| TX02 | 00-10 | Route to Output 2 | `TX02 03\r\n` (Input 3 → Output 2) |
| TX03 | 00-10 | Route to Output 3 | `TX03 00\r\n` (Turn off Output 3) |
| TX04 | 00-10 | Route to Output 4 | `TX04 01\r\n` (Input 1 → Output 4) |
| TX05 | 00-10 | Route to Output 5 | `TX05 10\r\n` (Input 10 → Output 5) |
| TX06 | 00-10 | Route to Output 6 | `TX06 02\r\n` (Input 2 → Output 6) |
| TX07 | 00-10 | Route to Output 7 | `TX07 07\r\n` (Input 7 → Output 7) |
| TX08 | 00-10 | Route to Output 8 | `TX08 04\r\n` (Input 4 → Output 8) |
| TX09 | 00-10 | Route to Output 9 | `TX09 06\r\n` (Input 6 → Output 9) |
| TX10 | 00-10 | Route to Output 10 | `TX10 09\r\n` (Input 9 → Output 10) |

**Parameter Values:**
- `00` = Turn output OFF
- `01-10` = Input number to route

## Audio Extraction

| Command | Parameter | Description | Example |
|---------|-----------|-------------|---------|
| AUDIO01 | H | SPDIF 1 select HDMI | `AUDIO01 H\r\n` |
| AUDIO01 | A | SPDIF 1 select ARC | `AUDIO01 A\r\n` |
| AUDIO02 | H | SPDIF 2 select HDMI | `AUDIO02 H\r\n` |
| AUDIO02 | A | SPDIF 2 select ARC | `AUDIO02 A\r\n` |

**Audio Sources:**
- `H` = HDMI audio
- `A` = ARC (Audio Return Channel)

## Scene Management

| Command | Parameter | Description | Example |
|---------|-----------|-------------|---------|
| SAVE | 01-10 | Save current config to scene | `SAVE 01\r\n` (Save to Scene 1) |
| LOAD | 01-10 | Load saved scene | `LOAD 05\r\n` (Load Scene 5) |

**Scenes:**
- 10 total scenes (01-10)
- Scenes save complete routing and audio configuration

## Panel Control

| Command | Parameter | Description | Example |
|---------|-----------|-------------|---------|
| LOCK | 00 | Unlock front panel & IR | `LOCK 00\r\n` |
| LOCK | 01 | Lock front panel & IR | `LOCK 01\r\n` |
| IRID | 00-09 | Set IR Remote ID | `IRID 04\r\n` (Set ID to 4) |

## Network Configuration

| Command | Parameter | Description | Example |
|---------|-----------|-------------|---------|
| DHCP | 00 | Disable DHCP | `DHCP 00\r\n` |
| DHCP | 01 | Enable DHCP | `DHCP 01\r\n` |
| IP_ADDRESS | n.n.n.n | Set IP address | `IP_ADDRESS 192.168.0.10\r\n` |
| SUBNET_MASK | n.n.n.n | Set subnet mask | `SUBNET_MASK 255.255.255.0\r\n` |
| GATEWAY | n.n.n.n | Set gateway | `GATEWAY 192.168.0.1\r\n` |
| MEDIA_TYPE | 00-02 | Set network speed | `MEDIA_TYPE 00\r\n` (Auto) |
| MAC_FILTER | 00-01 | MAC filter on/off | `MAC_FILTER 01\r\n` (Enable) |

**Media Type Values:**
- `00` = Auto-detect
- `01` = 10M
- `02` = 100M

## Status Queries

| Command | Description | Example | Response |
|---------|-------------|---------|----------|
| STATUS | Get connection status | `STATUS\r\n` | Status information |
| VERSION | Get firmware version | `VERSION\r\n` | Version details |
| HELP | Get command help | `HELP\r\n` | Available commands |

## Quick Command Examples

### Basic Routing
```
TX01 03\r\n    # Input 3 to Output 1
TX02 05\r\n    # Input 5 to Output 2
TX03 00\r\n    # Turn off Output 3
```

### Mirror Input to Multiple Outputs
```
TX01 02\r\n    # Input 2 to Output 1
TX02 02\r\n    # Input 2 to Output 2
TX03 02\r\n    # Input 2 to Output 3
TX04 02\r\n    # Input 2 to Output 4
```

### Turn Off All Outputs
```
TX01 00\r\n
TX02 00\r\n
TX03 00\r\n
TX04 00\r\n
TX05 00\r\n
TX06 00\r\n
TX07 00\r\n
TX08 00\r\n
TX09 00\r\n
TX10 00\r\n
```

### Scene Operations
```
SAVE 01\r\n    # Save current config to Scene 1
LOAD 01\r\n    # Load Scene 1
SAVE 02\r\n    # Save to Scene 2
LOAD 02\r\n    # Load Scene 2
```

### Audio Configuration
```
AUDIO01 H\r\n  # SPDIF 1 uses HDMI audio
AUDIO02 A\r\n  # SPDIF 2 uses ARC audio
```

### Network Setup
```
DHCP 00\r\n                         # Disable DHCP
IP_ADDRESS 192.168.1.100\r\n        # Set static IP
SUBNET_MASK 255.255.255.0\r\n       # Set subnet
GATEWAY 192.168.1.1\r\n             # Set gateway
MEDIA_TYPE 02\r\n                   # 100M network
```

### Power & Lock
```
POWER 01\r\n   # Power on
LOCK 01\r\n    # Lock panel
IRID 05\r\n    # Set IR ID to 5
```

## Common Routing Patterns

### Home Theater (Outputs 6-8)
```
TX06 02\r\n    # Blu-ray player to projector 1
TX07 02\r\n    # Blu-ray player to projector 2
TX08 02\r\n    # Blu-ray player to projector 3
AUDIO01 H\r\n  # Extract HDMI audio for surround
SAVE 01\r\n    # Save as Scene 1
```

### All TVs Show Cable (Input 1)
```
TX01 01\r\n
TX02 01\r\n
TX03 01\r\n
TX04 01\r\n
TX05 01\r\n
TX06 01\r\n
TX07 01\r\n
TX08 01\r\n
TX09 01\r\n
TX10 01\r\n
SAVE 02\r\n    # Save as Scene 2
```

### Living Room Setup (Outputs 1-2)
```
TX01 03\r\n    # Apple TV to main display
TX02 03\r\n    # Apple TV to secondary
AUDIO01 A\r\n  # Use ARC from TV for soundbar
SAVE 03\r\n    # Save as Scene 3
```

### Default 1:1 Routing
```
TX01 01\r\n
TX02 02\r\n
TX03 03\r\n
TX04 04\r\n
TX05 05\r\n
TX06 06\r\n
TX07 07\r\n
TX08 08\r\n
TX09 09\r\n
TX10 10\r\n
SAVE 10\r\n    # Save as Scene 10
```

## Response Codes

| Response | Meaning |
|----------|---------|
| OK | Command executed successfully |
| NG | Command failed or invalid parameter |

## Connection Settings

### RS-232 (CONSOLE Port)
- **Baud Rate**: 9600
- **Data Bits**: 8
- **Stop Bits**: 1
- **Parity**: None
- **Flow Control**: None
- **Connector**: DB9 Female

### TCP/IP
- **Default IP**: 192.168.0.10
- **Port**: 47011
- **Protocol**: Same commands as RS-232

## Command Timing

- **Minimum delay between commands**: 50ms recommended
- **Power on delay**: Wait 5 seconds before sending commands after power on
- **Scene load time**: ~1 second
- **Network config changes**: Take effect immediately

## Error Handling

If you receive "NG" response:

1. **Check parameter range**
   - Output: 01-10
   - Input: 00-10
   - Scene: 01-10
   - IR ID: 00-09

2. **Check command format**
   - Space between command and parameter
   - Proper delimiter (\r\n)
   - Case doesn't matter

3. **Check device state**
   - Device must be powered on (except for POWER command)
   - Panel must be unlocked for manual control
   - Network settings valid

4. **Retry command**
   - Wait 100ms and retry
   - Check cable connection

## Notes

- In standby mode, only POWER 01 command works via IR remote
- Front panel must have ENTER pressed to confirm menu changes
- SCENE button on panel allows loading presets directly
- ALL button routes same input to all outputs
- OFF button turns off selected outputs
- Commands are queued if sent rapidly; allow time between commands
- Maximum 4 simultaneous Web GUI connections
- MAC filtering must be configured via Web GUI
- Firmware updates via USB only

## Hexadecimal Values

For low-level programming:

| Character | Hex Value |
|-----------|-----------|
| CR (\\r) | 0x0D |
| LF (\\n) | 0x0A |
| Space | 0x20 |

Example command in hex:
```
"TX01 05\r\n" = 54 58 30 31 20 30 35 0D 0A
```

## Quick Reference Card

**Most Common Commands:**

| Action | Command |
|--------|---------|
| Route Input 3 to Output 1 | TX01 03 |
| Turn off Output 5 | TX05 00 |
| Load Scene 1 | LOAD 01 |
| Save Scene 1 | SAVE 01 |
| SPDIF 1 HDMI audio | AUDIO01 H |
| SPDIF 2 ARC audio | AUDIO02 A |
| Power On | POWER 01 |
| Power Off | POWER 00 |
| Lock Panel | LOCK 01 |
| Get Status | STATUS |

---

**Remember**: All commands must end with \\r\\n (CR+LF)

For detailed programming examples, see EXAMPLES.md
