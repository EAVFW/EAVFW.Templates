#!/usr/bin/env node
// Cross-platform setup script - used by dotnet new post-actions
// Only applies the NextJS template and generates certs.
// npm install, build, and db-create are handled by Aspire at runtime.
var childProcess = require('child_process');
var os = require('os');
var fs = require('fs');
var path = require('path');

var isWindows = os.platform() === 'win32';

function run(cmd) {
  console.log('> ' + cmd);
  childProcess.execSync(cmd, { stdio: 'inherit', shell: true });
}

run('npm run eavfw-nextjs');

// Run cert generation if available (created by eavfw-nextjs template)
var gencertCmd = path.join('scripts', 'gencert.cmd');
var gencertSh = path.join('scripts', 'gencert.sh');

if (isWindows && fs.existsSync(gencertCmd)) {
  run('call "' + gencertCmd + '"');
} else if (fs.existsSync(gencertSh)) {
  run('bash "' + gencertSh + '"');
} else if (fs.existsSync(gencertCmd)) {
  console.warn('Warning: Only gencert.cmd found. Skipping cert generation on non-Windows.');
}
