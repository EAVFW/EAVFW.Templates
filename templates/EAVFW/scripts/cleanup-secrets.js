#!/usr/bin/env node
// Clean up script files that may contain secrets after template setup
var fs = require('fs');
var path = require('path');

['mail-setpassword.cmd', 'mail-setpassword.sh'].forEach(function(f) {
  try { fs.unlinkSync(path.join(__dirname, f)); } catch(e) {}
});

// Self-delete
try { fs.unlinkSync(__filename); } catch(e) {}
