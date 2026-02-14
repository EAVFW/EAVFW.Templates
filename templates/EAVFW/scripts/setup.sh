#!/bin/bash
set -e

npm --version
npm run eavfw-nextjs

# Run cert generation if available (created by eavfw-nextjs template)
if [ -f scripts/gencert.sh ]; then
  bash scripts/gencert.sh
elif [ -f scripts/gencert.cmd ]; then
  echo "Warning: Only gencert.cmd found. Skipping cert generation on non-Windows."
  echo "Create scripts/gencert.sh or run manually."
fi

npm install --force
npm run build
npm run db-create
