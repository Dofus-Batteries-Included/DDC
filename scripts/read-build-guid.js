#!/usr/bin/env node

/**
 * Script that runs the game executable once after BepInEx has been installed.
 * During the first startup, BepInEx generates the interoperability assemblies that are used by the plugins to communicate with the IL2CPP domain.
 * BepInEx uses Cpp2IL and IL2CppInterop to generate the assemblies. We could use those tools directly instead of relying on BepInEx but it's easier to let BepInEx do its thing.
 */

const fs = require('fs');
const path = require('path');

if (process.argv.length < 3) {
    console.log('Usage: node read-build-guid.js <boot.config path>')
    process.exit(1);
}

const bootConfigPath = path.resolve(process.argv[2]);

if (!fs.existsSync(bootConfigPath)) {
    console.log(`File ${bootConfigPath} does not exist.`);
    process.exit(1);
}

var bootConfigContent = fs.readFileSync(bootConfigPath, 'utf8');
var match = bootConfigContent.match(/build-guid=(?<guid>.*)/)
if (!match) {
    console.log(`Could not find build guid in boot.config file at ${bootConfigPath}`);
    process.exit(1);
}

console.log(match.groups["guid"]);