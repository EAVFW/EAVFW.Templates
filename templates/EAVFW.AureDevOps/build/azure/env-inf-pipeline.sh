#!/bin/bash
set -Eeuxo pipefail


source pipeline-setup.sh;



#sqlserverdomainname=$(echo "$sqlinfo" | jq -r '.fullyQualifiedDomainName')

#AppName="${prefixShort}-WebApp-${projectName}-${locationShort}-${projectEnv}"

packagePath="${publishFolder}/__EAVFW__.__MainApp__.zip"
#appsettings=(
#"APPINSIGHTS_INSTRUMENTATIONKEY=${instrumentationKey}"
#"APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=${instrumentationKey}"
#"ApplicationInsightsAgent_EXTENSION_VERSION=~2",
#"ConnectionString=Data Source=${sqlserverdomainname};Initial Catalog=${sqldbname};Authentication=Active Directory Service Principal"
#)



defaultHostName=$(az webapp show --name ${AppName} | jq -r '.defaultHostName')
#appsettings+=("HostUrl=https://${defaultHostName}")

#echo "Configuring ${AppName} for https://${defaultHostName}"
#az webapp config appsettings set --name ${AppName}  --settings "${appsettings[@]}"

echo "Deploying ${AppName} as zip"
./retry3.sh az webapp deployment source config-zip -n ${AppName} --src "${packagePath}" --timeout 60

echo "Querying liveness endpoint for https://${defaultHostName}/.well-known/live"
./retry3.sh curl  -I -X GET --fail --show-error "https://${defaultHostName}/.well-known/live"
