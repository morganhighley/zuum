# Zuum H10X10-4K6G - Programming Examples

Practical examples for Crestron programmers using the H10X10-4K6G driver.

## Table of Contents

1. [Basic Setup](#basic-setup)
2. [Common Routing Scenarios](#common-routing-scenarios)
3. [Scene Programming](#scene-programming)
4. [Audio Extraction](#audio-extraction)
5. [Advanced Patterns](#advanced-patterns)
6. [Error Handling](#error-handling)
7. [Integration Examples](#integration-examples)

## Basic Setup

### Initialize the Controller

```javascript
const { ZuumH10X10Controller } = require('./zuum-h10x10-control.js');

// Create controller instance
const matrix = new ZuumH10X10Controller();

// Power on the matrix
const powerCmd = matrix.powerOn();
// Send powerCmd to device via RS-232
```

### Simple Routing

```javascript
// Route Input 1 to Output 1
const cmd1 = matrix.routeInputToOutput(1, 1);
// Returns: "TX01 01\r\n"

// Route Input 5 to Output 3
const cmd2 = matrix.routeInputToOutput(3, 5);
// Returns: "TX03 05\r\n"

// Turn off Output 7
const cmd3 = matrix.turnOffOutput(7);
// Returns: "TX07 00\r\n"
```

## Common Routing Scenarios

### Scenario 1: Home Theater System

```javascript
// Inputs:
// 1 = Cable Box
// 2 = Blu-ray Player
// 3 = Apple TV
// 4 = Gaming Console
// 5 = Chromecast

// Outputs:
// 1 = Living Room Main TV
// 2 = Living Room Secondary Display
// 3 = Master Bedroom TV
// 4 = Guest Bedroom TV
// 5 = Kitchen TV
// 6-8 = Home Theater (3 projectors)
// 9-10 = Game Room

// Living room watching Apple TV
matrix.routeInputToOutput(1, 3); // Living Room Main <- Apple TV
matrix.routeInputToOutput(2, 3); // Living Room Secondary <- Apple TV

// Master bedroom watching Cable
matrix.routeInputToOutput(3, 1); // Master Bedroom <- Cable Box

// Kitchen watching Chromecast
matrix.routeInputToOutput(5, 5); // Kitchen <- Chromecast

// Home theater watching Blu-ray
matrix.routeInputToOutput(6, 2); // Theater 1 <- Blu-ray
matrix.routeInputToOutput(7, 2); // Theater 2 <- Blu-ray
matrix.routeInputToOutput(8, 2); // Theater 3 <- Blu-ray

// Game room with gaming console
matrix.routeInputToOutput(9, 4);  // Game Room 1 <- Gaming
matrix.routeInputToOutput(10, 4); // Game Room 2 <- Gaming

// Save this configuration as Scene 1
matrix.saveScene(1);
```

### Scenario 2: Commercial Application (Sports Bar)

```javascript
// Inputs:
// 1-5 = Different Sports Channels/Sources

// Outputs:
// 1-10 = TVs around the venue

// Show different games on different TVs
const commands = [
    matrix.routeInputToOutput(1, 1),  // TV1 <- Source 1 (NBA)
    matrix.routeInputToOutput(2, 1),  // TV2 <- Source 1 (NBA)
    matrix.routeInputToOutput(3, 1),  // TV3 <- Source 1 (NBA)
    matrix.routeInputToOutput(4, 2),  // TV4 <- Source 2 (NFL)
    matrix.routeInputToOutput(5, 2),  // TV5 <- Source 2 (NFL)
    matrix.routeInputToOutput(6, 2),  // TV6 <- Source 2 (NFL)
    matrix.routeInputToOutput(7, 3),  // TV7 <- Source 3 (MLB)
    matrix.routeInputToOutput(8, 3),  // TV8 <- Source 3 (MLB)
    matrix.routeInputToOutput(9, 4),  // TV9 <- Source 4 (Soccer)
    matrix.routeInputToOutput(10, 5)  // TV10 <- Source 5 (News)
];

// Execute all commands
commands.forEach(cmd => sendToDevice(cmd));
```

### Scenario 3: All TVs Show Same Source

```javascript
// Show Super Bowl on all TVs
const cmds = matrix.routeInputToAll(1);
// This generates 10 commands routing Input 1 to all outputs

// Or use the helper method
const cmds2 = matrix.mirrorInput1ToAll();
```

### Scenario 4: Conference Room

```javascript
// Inputs:
// 1 = Laptop 1 (HDMI)
// 2 = Laptop 2 (HDMI)
// 3 = Document Camera
// 4 = Wireless Presentation System

// Outputs:
// 1 = Main Projector
// 2 = Confidence Monitor 1
// 3 = Confidence Monitor 2
// 4 = Recording System

// Presenter using Laptop 1
matrix.routeInputToOutput(1, 1);  // Main Projector <- Laptop 1
matrix.routeInputToOutput(2, 1);  // Confidence 1 <- Laptop 1
matrix.routeInputToOutput(3, 1);  // Confidence 2 <- Laptop 1
matrix.routeInputToOutput(4, 1);  // Recording <- Laptop 1

// Or use the helper
matrix.routeInputToMultipleOutputs([1, 2, 3, 4], 1);
```

## Scene Programming

### Creating Multiple Scenes

```javascript
// Scene 1: Morning News (all TVs show news)
matrix.routeInputToAll(1);
matrix.saveScene(1);

// Scene 2: Movie Night (theater mode)
matrix.routeInputToMultipleOutputs([6, 7, 8], 2);
matrix.turnOffMultipleOutputs([1, 2, 3, 4, 5, 9, 10]);
matrix.saveScene(2);

// Scene 3: Game Day (sports on most TVs)
matrix.createCustomRouting({
    1: 5,  // Sports
    2: 5,
    3: 5,
    4: 5,
    5: 5,
    6: 5,
    7: 3,  // Kids room - cartoons
    8: 0,  // Off
    9: 0,  // Off
    10: 0  // Off
});
matrix.saveScene(3);

// Scene 4: All Off
matrix.turnOffAllOutputs();
matrix.saveScene(4);

// Scene 5: Default 1:1 routing
matrix.setDefaultRouting();
matrix.saveScene(5);
```

### Recalling Scenes

```javascript
// Load Scene 1 (Morning News)
matrix.loadScene(1);

// Load Scene 2 (Movie Night)
matrix.loadScene(2);

// Create scene selector buttons
function selectScene(sceneNumber) {
    if (sceneNumber >= 1 && sceneNumber <= 10) {
        return matrix.loadScene(sceneNumber);
    }
}

// Button press handlers
button1.onPress = () => sendToDevice(selectScene(1));
button2.onPress = () => sendToDevice(selectScene(2));
button3.onPress = () => sendToDevice(selectScene(3));
```

## Audio Extraction

### Basic Audio Setup

```javascript
// Extract HDMI audio from matrix to audio system
matrix.spdif1SelectHDMI();  // SPDIF 1 -> Audio System 1

// Use ARC from TV for second audio system
matrix.spdif2SelectARC();   // SPDIF 2 -> Audio System 2
```

### Dynamic Audio Switching

```javascript
// Home Theater Mode: Use HDMI audio for surround sound
function activateTheaterMode() {
    // Route Blu-ray to theater outputs
    matrix.routeInputToMultipleOutputs([6, 7, 8], 2);

    // Extract HDMI audio to surround system
    matrix.spdif1SelectHDMI();

    return 'Theater Mode Activated';
}

// Living Room Mode: Use TV's ARC for soundbar
function activateLivingRoomMode() {
    // Route Apple TV to living room
    matrix.routeInputToMultipleOutputs([1, 2], 3);

    // Use ARC from TV for soundbar
    matrix.spdif1SelectARC();

    return 'Living Room Mode Activated';
}
```

### Multi-Zone Audio

```javascript
// Zone 1 (Living Room): ARC from TV
matrix.spdif1SelectARC();

// Zone 2 (Theater): HDMI direct audio
matrix.spdif2SelectHDMI();

// Create audio zone buttons
function setAudioZone(zone, source) {
    const audioSource = source === 'HDMI' ? 'H' : 'A';
    return matrix.selectAudioSource(zone, audioSource);
}

// Usage
setAudioZone(1, 'ARC');   // Zone 1 uses ARC
setAudioZone(2, 'HDMI');  // Zone 2 uses HDMI
```

## Advanced Patterns

### Zone-Based Control

```javascript
const { RoutingPatterns } = require('./zuum-h10x10-control.js');
const patterns = RoutingPatterns.matrix(matrix);

// Define zones
const zones = {
    livingRoom: [1, 2],      // Outputs 1-2
    bedroom: [3],             // Output 3
    kitchen: [4, 5],         // Outputs 4-5
    theater: [6, 7, 8],      // Outputs 6-8
    gameRoom: [9, 10]        // Outputs 9-10
};

// Route source to specific zone
function routeToZone(zoneName, input) {
    const outputs = zones[zoneName];
    return matrix.routeInputToMultipleOutputs(outputs, input);
}

// Usage
routeToZone('livingRoom', 3);  // Apple TV to Living Room
routeToZone('theater', 2);     // Blu-ray to Theater
routeToZone('gameRoom', 4);    // Gaming Console to Game Room
```

### Source Priority System

```javascript
// If multiple requests, prioritize based on source
const sourcePriority = {
    emergency: 1,    // Emergency broadcast
    presentation: 2, // Business presentation
    sports: 3,       // Sports events
    entertainment: 4 // Movies/TV
};

function routeWithPriority(output, input, priority) {
    // Store current routing request with priority
    const request = {
        output: output,
        input: input,
        priority: priority,
        timestamp: Date.now()
    };

    // Implement priority logic
    // (This is pseudocode - implement based on your system)
    if (canRoute(request)) {
        return matrix.routeInputToOutput(output, input);
    }
}
```

### Scheduled Routing

```javascript
// Schedule routing changes
class ScheduledRouting {
    constructor(matrix) {
        this.matrix = matrix;
        this.schedule = [];
    }

    // Add scheduled route
    addSchedule(time, action) {
        this.schedule.push({ time, action });
    }

    // Execute scheduled routes
    executeSchedule() {
        const now = new Date();
        const currentTime = now.getHours() * 60 + now.getMinutes();

        this.schedule.forEach(item => {
            if (item.time === currentTime) {
                item.action();
            }
        });
    }
}

// Usage
const scheduler = new ScheduledRouting(matrix);

// 7:00 AM - Morning news on all TVs
scheduler.addSchedule(420, () => {
    matrix.routeInputToAll(1);
    matrix.saveScene(1);
});

// 6:00 PM - Evening entertainment
scheduler.addSchedule(1080, () => {
    matrix.loadScene(2);
});

// Run scheduler every minute
setInterval(() => scheduler.executeSchedule(), 60000);
```

### Failover Routing

```javascript
// If primary source fails, switch to backup
function routeWithFailover(output, primaryInput, backupInput) {
    // Try primary source
    const cmd1 = matrix.routeInputToOutput(output, primaryInput);

    // Monitor for signal
    // If no signal detected, switch to backup
    setTimeout(() => {
        if (!signalDetected(output)) {
            const cmd2 = matrix.routeInputToOutput(output, backupInput);
            sendToDevice(cmd2);
        }
    }, 5000); // Wait 5 seconds before switching

    return cmd1;
}
```

## Error Handling

### Command Validation

```javascript
function safeRoute(output, input) {
    try {
        const cmd = matrix.routeInputToOutput(output, input);
        const response = sendToDeviceSync(cmd);
        const result = matrix.parseResponse(response);

        if (!result.success) {
            console.error('Routing failed:', result.message);
            // Try again or alert user
            return false;
        }

        return true;
    } catch (error) {
        console.error('Routing error:', error.message);
        return false;
    }
}
```

### Response Parsing

```javascript
function executeCommand(cmd) {
    const response = sendToDevice(cmd);
    const parsed = matrix.parseResponse(response);

    if (parsed.success) {
        console.log('✓ Command successful');
        return true;
    } else {
        console.error('✗ Command failed:', parsed.message);
        // Handle error
        return false;
    }
}
```

### Retry Logic

```javascript
async function executeWithRetry(cmd, maxRetries = 3) {
    for (let i = 0; i < maxRetries; i++) {
        try {
            const response = await sendToDevice(cmd);
            const result = matrix.parseResponse(response);

            if (result.success) {
                return result;
            }

            // Wait before retry
            await delay(1000 * (i + 1));
        } catch (error) {
            console.error(`Attempt ${i + 1} failed:`, error);
        }
    }

    throw new Error('Command failed after retries');
}
```

## Integration Examples

### Crestron SIMPL+ Integration

```c
// SIMPL+ Example
#DEFINE_CONSTANT MAX_OUTPUTS 10
#DEFINE_CONSTANT MAX_INPUTS 10

STRING_INPUT Route_Input[MAX_OUTPUTS];
DIGITAL_OUTPUT Route_FB[MAX_OUTPUTS][MAX_INPUTS];
STRING_OUTPUT Command_Out;

INTEGER output, input;

THREADSAFE CHANGE Route_Input {
    // Parse input format: "output,input"
    output = ATOI(LEFT(Route_Input, 2));
    input = ATOI(RIGHT(Route_Input, 2));

    // Format command: TX01 05\r\n
    MAKESTRING(Command_Out, "TX%02d %02d\r\n", output, input);
}
```

### Crestron C# Integration

```csharp
using System;
using Crestron.SimplSharp;

public class ZuumMatrixController
{
    private ComPort serialPort;

    public void RouteInput(ushort output, ushort input)
    {
        if (output < 1 || output > 10 || input < 0 || input > 10)
        {
            ErrorLog.Error("Invalid routing parameters");
            return;
        }

        string command = string.Format("TX{0:D2} {1:D2}\r\n", output, input);
        serialPort.Send(command);
    }

    public void LoadScene(ushort sceneNumber)
    {
        if (sceneNumber < 1 || sceneNumber > 10)
        {
            ErrorLog.Error("Invalid scene number");
            return;
        }

        string command = string.Format("LOAD {0:D2}\r\n", sceneNumber);
        serialPort.Send(command);
    }
}
```

### Touch Panel Integration

```javascript
// Button press handlers for touch panel

// Input selection buttons
function onInputButtonPress(inputNumber) {
    // Store selected input
    selectedInput = inputNumber;

    // Update UI feedback
    highlightInputButton(inputNumber);
}

// Output selection buttons
function onOutputButtonPress(outputNumber) {
    if (selectedInput > 0) {
        // Route selected input to this output
        const cmd = matrix.routeInputToOutput(outputNumber, selectedInput);
        sendToDevice(cmd);

        // Update UI
        updateOutputDisplay(outputNumber, selectedInput);
    }
}

// Scene recall buttons
function onSceneButtonPress(sceneNumber) {
    const cmd = matrix.loadScene(sceneNumber);
    sendToDevice(cmd);

    // Show feedback
    showMessage(`Scene ${sceneNumber} loaded`);
}

// All Off button
function onAllOffPress() {
    const cmds = matrix.turnOffAllOutputs();
    cmds.forEach(cmd => sendToDevice(cmd));

    // Update UI
    clearAllOutputDisplays();
}
```

### Web Interface Integration

```javascript
// Express.js API endpoints

const express = require('express');
const app = express();
const matrix = new ZuumH10X10Controller();

// Route input to output
app.post('/api/route', (req, res) => {
    const { output, input } = req.body;

    try {
        const cmd = matrix.routeInputToOutput(output, input);
        const result = sendToDevice(cmd);

        res.json({
            success: true,
            command: cmd,
            output: output,
            input: input
        });
    } catch (error) {
        res.status(400).json({
            success: false,
            error: error.message
        });
    }
});

// Load scene
app.post('/api/scene/:number', (req, res) => {
    const sceneNumber = parseInt(req.params.number);

    try {
        const cmd = matrix.loadScene(sceneNumber);
        sendToDevice(cmd);

        res.json({
            success: true,
            scene: sceneNumber
        });
    } catch (error) {
        res.status(400).json({
            success: false,
            error: error.message
        });
    }
});

// Get current routing
app.get('/api/routing', (req, res) => {
    const routing = matrix.getCurrentRouting();
    res.json(routing);
});

app.listen(3000);
```

## Best Practices

1. **Always validate inputs** before sending commands
2. **Parse responses** to confirm command success
3. **Implement error handling** for failed commands
4. **Use scenes** for complex routing configurations
5. **Provide user feedback** on all routing changes
6. **Log all commands** for debugging
7. **Test routing patterns** before deployment
8. **Document custom configurations** for maintenance
9. **Implement retry logic** for critical commands
10. **Keep firmware updated** for best performance

## Performance Tips

- **Batch commands** when possible to reduce serial traffic
- **Use scenes** instead of multiple individual routes
- **Cache current state** to avoid unnecessary commands
- **Implement command queuing** for busy systems
- **Add delays** between rapid command sequences
- **Monitor device responses** for errors

---

For more examples and support, refer to the main README.md and device manual.
