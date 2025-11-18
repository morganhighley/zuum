/**
 * Zuum Media H10X10-4K6G HDMI Matrix Switcher Control Module
 * Complete control library for Crestron Home integration
 * Version: 1.0.0
 */

class ZuumH10X10Controller {
    constructor() {
        this.outputs = 10;
        this.inputs = 10;
        this.scenes = 10;
        this.delimiter = '\r\n';
        this.currentRouting = {};
        this.audioSources = {};
        this.powerState = false;
        this.panelLocked = false;
        this.irID = 4; // Default IR ID

        // Initialize routing state
        for (let i = 1; i <= this.outputs; i++) {
            this.currentRouting[i] = 0; // 0 = off
        }

        // Initialize audio sources
        this.audioSources[1] = 'H'; // HDMI
        this.audioSources[2] = 'H'; // HDMI
    }

    /**
     * Format command with proper delimiter
     * @param {string} cmd - Command string
     * @returns {string} - Formatted command
     */
    formatCommand(cmd) {
        return cmd + this.delimiter;
    }

    /**
     * Power Control
     */
    powerOn() {
        return this.formatCommand('POWER 01');
    }

    powerOff() {
        return this.formatCommand('POWER 00');
    }

    togglePower() {
        this.powerState = !this.powerState;
        return this.powerState ? this.powerOn() : this.powerOff();
    }

    /**
     * Routing Control - Route single input to single output
     * @param {number} output - Output number (1-10)
     * @param {number} input - Input number (0-10, 0=OFF)
     * @returns {string} - Formatted command
     */
    routeInputToOutput(output, input) {
        if (output < 1 || output > this.outputs) {
            throw new Error(`Invalid output: ${output}. Must be 1-${this.outputs}`);
        }
        if (input < 0 || input > this.inputs) {
            throw new Error(`Invalid input: ${input}. Must be 0-${this.inputs}`);
        }

        const outputPadded = output.toString().padStart(2, '0');
        const inputPadded = input.toString().padStart(2, '0');
        this.currentRouting[output] = input;

        return this.formatCommand(`TX${outputPadded} ${inputPadded}`);
    }

    /**
     * Route input to all outputs
     * @param {number} input - Input number (0-10, 0=OFF)
     * @returns {Array<string>} - Array of commands
     */
    routeInputToAll(input) {
        const commands = [];
        for (let output = 1; output <= this.outputs; output++) {
            commands.push(this.routeInputToOutput(output, input));
        }
        return commands;
    }

    /**
     * Route input to multiple outputs
     * @param {Array<number>} outputs - Array of output numbers
     * @param {number} input - Input number (0-10)
     * @returns {Array<string>} - Array of commands
     */
    routeInputToMultipleOutputs(outputs, input) {
        const commands = [];
        for (const output of outputs) {
            commands.push(this.routeInputToOutput(output, input));
        }
        return commands;
    }

    /**
     * Turn off specific output
     * @param {number} output - Output number (1-10)
     * @returns {string} - Formatted command
     */
    turnOffOutput(output) {
        return this.routeInputToOutput(output, 0);
    }

    /**
     * Turn off all outputs
     * @returns {Array<string>} - Array of commands
     */
    turnOffAllOutputs() {
        return this.routeInputToAll(0);
    }

    /**
     * Turn off multiple outputs
     * @param {Array<number>} outputs - Array of output numbers
     * @returns {Array<string>} - Array of commands
     */
    turnOffMultipleOutputs(outputs) {
        return this.routeInputToMultipleOutputs(outputs, 0);
    }

    /**
     * Audio Control - Select audio source for SPDIF output
     * @param {number} spdifOutput - SPDIF output (1-2)
     * @param {string} source - Audio source ('H' for HDMI, 'A' for ARC)
     * @returns {string} - Formatted command
     */
    selectAudioSource(spdifOutput, source) {
        if (spdifOutput < 1 || spdifOutput > 2) {
            throw new Error(`Invalid SPDIF output: ${spdifOutput}. Must be 1 or 2`);
        }
        if (source !== 'H' && source !== 'A') {
            throw new Error(`Invalid audio source: ${source}. Must be 'H' (HDMI) or 'A' (ARC)`);
        }

        const outputPadded = spdifOutput.toString().padStart(2, '0');
        this.audioSources[spdifOutput] = source;

        return this.formatCommand(`AUDIO${outputPadded} ${source}`);
    }

    /**
     * Set SPDIF 1 to HDMI audio
     */
    spdif1SelectHDMI() {
        return this.selectAudioSource(1, 'H');
    }

    /**
     * Set SPDIF 1 to ARC audio
     */
    spdif1SelectARC() {
        return this.selectAudioSource(1, 'A');
    }

    /**
     * Set SPDIF 2 to HDMI audio
     */
    spdif2SelectHDMI() {
        return this.selectAudioSource(2, 'H');
    }

    /**
     * Set SPDIF 2 to ARC audio
     */
    spdif2SelectARC() {
        return this.selectAudioSource(2, 'A');
    }

    /**
     * Scene Management - Save current routing to scene
     * @param {number} sceneNumber - Scene number (1-10)
     * @returns {string} - Formatted command
     */
    saveScene(sceneNumber) {
        if (sceneNumber < 1 || sceneNumber > this.scenes) {
            throw new Error(`Invalid scene number: ${sceneNumber}. Must be 1-${this.scenes}`);
        }

        const scenePadded = sceneNumber.toString().padStart(2, '0');
        return this.formatCommand(`SAVE ${scenePadded}`);
    }

    /**
     * Scene Management - Load saved scene
     * @param {number} sceneNumber - Scene number (1-10)
     * @returns {string} - Formatted command
     */
    loadScene(sceneNumber) {
        if (sceneNumber < 1 || sceneNumber > this.scenes) {
            throw new Error(`Invalid scene number: ${sceneNumber}. Must be 1-${this.scenes}`);
        }

        const scenePadded = sceneNumber.toString().padStart(2, '0');
        return this.formatCommand(`LOAD ${scenePadded}`);
    }

    /**
     * Panel Lock Control
     * @param {boolean} lock - True to lock, false to unlock
     * @returns {string} - Formatted command
     */
    setPanelLock(lock) {
        this.panelLocked = lock;
        const state = lock ? '01' : '00';
        return this.formatCommand(`LOCK ${state}`);
    }

    /**
     * Lock front panel and IR remote
     */
    lockPanel() {
        return this.setPanelLock(true);
    }

    /**
     * Unlock front panel and IR remote
     */
    unlockPanel() {
        return this.setPanelLock(false);
    }

    /**
     * Set IR Remote ID
     * @param {number} id - IR ID (0-9)
     * @returns {string} - Formatted command
     */
    setIRID(id) {
        if (id < 0 || id > 9) {
            throw new Error(`Invalid IR ID: ${id}. Must be 0-9`);
        }

        this.irID = id;
        const idPadded = id.toString().padStart(2, '0');
        return this.formatCommand(`IRID ${idPadded}`);
    }

    /**
     * Network Configuration - Set DHCP
     * @param {boolean} enable - True to enable DHCP, false to disable
     * @returns {string} - Formatted command
     */
    setDHCP(enable) {
        const state = enable ? '01' : '00';
        return this.formatCommand(`DHCP ${state}`);
    }

    /**
     * Network Configuration - Set IP Address
     * @param {string} ip - IP address (e.g., "192.168.0.10")
     * @returns {string} - Formatted command
     */
    setIPAddress(ip) {
        if (!this.validateIPAddress(ip)) {
            throw new Error(`Invalid IP address: ${ip}`);
        }
        return this.formatCommand(`IP_ADDRESS ${ip}`);
    }

    /**
     * Network Configuration - Set Subnet Mask
     * @param {string} mask - Subnet mask (e.g., "255.255.255.0")
     * @returns {string} - Formatted command
     */
    setSubnetMask(mask) {
        if (!this.validateIPAddress(mask)) {
            throw new Error(`Invalid subnet mask: ${mask}`);
        }
        return this.formatCommand(`SUBNET_MASK ${mask}`);
    }

    /**
     * Network Configuration - Set Gateway
     * @param {string} gateway - Gateway IP (e.g., "192.168.0.1")
     * @returns {string} - Formatted command
     */
    setGateway(gateway) {
        if (!this.validateIPAddress(gateway)) {
            throw new Error(`Invalid gateway: ${gateway}`);
        }
        return this.formatCommand(`GATEWAY ${gateway}`);
    }

    /**
     * Network Configuration - Set Media Type (Network Speed)
     * @param {string} type - Media type ('AUTO', '10M', '100M')
     * @returns {string} - Formatted command
     */
    setMediaType(type) {
        const typeMap = {
            'AUTO': '00',
            '10M': '01',
            '100M': '02'
        };

        if (!typeMap[type]) {
            throw new Error(`Invalid media type: ${type}. Must be 'AUTO', '10M', or '100M'`);
        }

        return this.formatCommand(`MEDIA_TYPE ${typeMap[type]}`);
    }

    /**
     * Network Configuration - Set MAC Filter
     * @param {boolean} enable - True to enable MAC filter, false to disable
     * @returns {string} - Formatted command
     */
    setMACFilter(enable) {
        const state = enable ? '01' : '00';
        return this.formatCommand(`MAC_FILTER ${state}`);
    }

    /**
     * Status Queries
     */
    getStatus() {
        return this.formatCommand('STATUS');
    }

    getVersion() {
        return this.formatCommand('VERSION');
    }

    getHelp() {
        return this.formatCommand('HELP');
    }

    /**
     * Utility Functions
     */
    validateIPAddress(ip) {
        const pattern = /^(\d{1,3}\.){3}\d{1,3}$/;
        if (!pattern.test(ip)) {
            return false;
        }

        const parts = ip.split('.');
        for (const part of parts) {
            const num = parseInt(part, 10);
            if (num < 0 || num > 255) {
                return false;
            }
        }
        return true;
    }

    /**
     * Parse feedback from device
     * @param {string} response - Response from device
     * @returns {object} - Parsed response
     */
    parseResponse(response) {
        const trimmed = response.trim();

        if (trimmed === 'OK') {
            return { success: true, message: 'Command executed successfully' };
        }

        if (trimmed === 'NG') {
            return { success: false, message: 'Command failed or invalid' };
        }

        // Parse status or version responses
        return { success: true, data: trimmed };
    }

    /**
     * Get current routing configuration
     * @returns {object} - Current routing state
     */
    getCurrentRouting() {
        return { ...this.currentRouting };
    }

    /**
     * Get current audio source configuration
     * @returns {object} - Current audio sources
     */
    getCurrentAudioSources() {
        return { ...this.audioSources };
    }

    /**
     * Quick Routing Presets
     */

    // Mirror input 1 to all outputs
    mirrorInput1ToAll() {
        return this.routeInputToAll(1);
    }

    // Mirror input 2 to all outputs
    mirrorInput2ToAll() {
        return this.routeInputToAll(2);
    }

    // Mirror input 3 to all outputs
    mirrorInput3ToAll() {
        return this.routeInputToAll(3);
    }

    // Mirror input 4 to all outputs
    mirrorInput4ToAll() {
        return this.routeInputToAll(4);
    }

    // Mirror input 5 to all outputs
    mirrorInput5ToAll() {
        return this.routeInputToAll(5);
    }

    // Set default 1:1 routing (Input 1 -> Output 1, Input 2 -> Output 2, etc.)
    setDefaultRouting() {
        const commands = [];
        for (let i = 1; i <= Math.min(this.inputs, this.outputs); i++) {
            commands.push(this.routeInputToOutput(i, i));
        }
        return commands;
    }

    /**
     * Batch Operations
     */

    /**
     * Execute multiple commands in sequence
     * @param {Array<string>} commands - Array of commands
     * @returns {Array<string>} - Array of formatted commands
     */
    batchCommands(commands) {
        return commands;
    }

    /**
     * Create custom routing configuration
     * @param {object} config - Routing configuration {output: input}
     * @returns {Array<string>} - Array of commands
     */
    createCustomRouting(config) {
        const commands = [];
        for (const [output, input] of Object.entries(config)) {
            commands.push(this.routeInputToOutput(parseInt(output), input));
        }
        return commands;
    }
}

/**
 * Helper class for common routing patterns
 */
class RoutingPatterns {
    static matrix(controller) {
        return {
            // Zone-based routing
            livingRoom: (input) => controller.routeInputToMultipleOutputs([1, 2], input),
            bedroom: (input) => controller.routeInputToOutput(3, input),
            kitchen: (input) => controller.routeInputToOutput(4, input),
            office: (input) => controller.routeInputToOutput(5, input),
            theater: (input) => controller.routeInputToMultipleOutputs([6, 7, 8], input),

            // Source-based quick selection
            cableBoxToAll: () => controller.routeInputToAll(1),
            blurayToTheater: () => controller.routeInputToMultipleOutputs([6, 7, 8], 2),
            streamingToLivingRoom: () => controller.routeInputToMultipleOutputs([1, 2], 3),

            // All off
            allOff: () => controller.turnOffAllOutputs()
        };
    }
}

// Export for use in Crestron Home
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { ZuumH10X10Controller, RoutingPatterns };
}
