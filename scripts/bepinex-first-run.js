#!/usr/bin/env node

/**
 * Script that runs the game executable once after BepInEx has been installed.
 * During the first startup, BepInEx generates the interoperability assemblies that are used by the plugins to communicate with the IL2CPP domain.
 * BepInEx uses Cpp2IL and IL2CppInterop to generate the assemblies. We could use those tools directly instead of relying on BepInEx but it's easier to let BepInEx do its thing.
 */

const fs = require('fs');
const path = require('path');
const spawn = require('child_process').spawn;

if (process.argv.length < 3) {
    console.log('Usage: node run-dofus-once.js <executable path>')
    process.exit(1);
}

const exePath = path.resolve(process.argv[2]);
const dirName = path.dirname(exePath);

if (!fs.existsSync(exePath)) {
    console.log(`File ${exePath} does not exist.`);
    process.exit(1);
}

console.log(`Will run dofus executable at: ${exePath}`)

const proc = spawn(exePath);
proc.stdout.pipe(process.stdout);
proc.stderr.pipe(process.stderr);

proc.on('close', function (code, signal) {
    process.exit(code);
});

console.log("Waiting for BepInEx to generate interop assemblies...")

const logFilePath = path.join(dirName, 'BepInEx', 'LogOutput.log');
setInterval(function () {
    if (!fs.existsSync(logFilePath)) {
        return;
    }
    
    const logFile = fs.readFileSync(logFilePath, {encoding: 'utf8', flag: 'r'});
    
    if (logFile.includes("Chainloader startup complete")) {
        console.log("BepInEx has generated interop assemblies, exiting.")    
        process.exit(0);
    }
}, 1000);