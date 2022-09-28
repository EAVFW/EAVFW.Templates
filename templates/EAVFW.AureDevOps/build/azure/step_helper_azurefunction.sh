#!/bin/bash
set -Eeuxo pipefail



#Create Storage Account (all lower case)
runtimeStorageAccountName=$(echo "${prefixShort}${storageAccountName}${locationShort}${projectEnv}" | sed 's/-//g' | awk '{print tolower($0)}')
accountNameLength=$(expr length "${runtimeStorageAccountName}")

if [ ${accountNameLength} -gt 23 ]
then
    COUNT=`expr ${accountNameLength} - 23`
	acnshort=$(echo "${storageAccountName}" | rev | cut -c ${COUNT}- | rev)
	runtimeStorageAccountName=$(echo "${prefixShort}${acnshort}${locationShort}${projectEnv}" | sed 's/-//g' | awk '{print tolower($0)}')
fi


echo "Setting Runtime storage account"
az storage account create -n ${runtimeStorageAccountName} --sku Standard_LRS --kind StorageV2
local_current_env_conn_string=$(az storage account show-connection-string -n ${runtimeStorageAccountName} --query 'connectionString' -o tsv)
dosecretexist=$(az keyvault secret list --vault-name ${keyVaultName} --query "[?name == 'storage-${storageAccountName}']")

if [[ "${dosecretexist}" == "[]" ]]; then
	secretUriWithVersion=$(az keyvault secret set --name storage-${storageAccountName} --vault-name ${keyVaultName} --value ${local_current_env_conn_string} | jq -r '.id')
else
	local_current_account_fromvault_conn_string=$(az keyvault secret show --name storage-${storageAccountName} --vault-name ${keyVaultName} --query 'value' -o tsv)
	if [ "$local_current_account_fromvault_conn_string" = "$local_current_env_conn_string" ]; then
		local_secretUriWithVersion=$(az keyvault secret show --name storage-${storageAccountName} --vault-name ${keyVaultName} --query 'id' -o tsv)
	else
		local_secretUriWithVersion=$(az keyvault secret set --name storage-${storageAccountName} --vault-name ${keyVaultName} --value ${local_current_env_conn_string} | jq -r '.id')
	fi
fi 

echo "Creating ${functionAppName}"
az functionapp create --functions-version 3 -n ${functionAppName} --storage-account ${runtimeStorageAccountName} --consumption-plan-location ${location} --app-insights-key ${instrumentationKey} --runtime dotnet -g ${rgname}
 
echo "Assigning Access for ${functionAppName}"
FunctionAppResourceId=$(az functionapp show --name ${functionAppName} | jq -r '.id')
az functionapp identity assign --name ${functionAppName} --resource-group ${rgname} --role Contributor --scope ${AEGResourceId}
az functionapp identity assign --name ${functionAppName} --resource-group ${rgname} --role Contributor --scope ${FunctionAppResourceId}
az functionapp identity assign --name ${functionAppName} --resource-group ${rgname} --role "Storage Blob Data Contributor" --scope ${AEGDeadLetterDestinationResourceId}
az functionapp identity assign --name ${functionAppName} --resource-group ${rgname} --role Contributor --scope ${AEGDeadLetterDestinationResourceId}

az keyvault set-policy --name ${keyVaultName} --secret-permissions get --object-id $(az functionapp show --name ${functionAppName} | jq -r '.identity.principalId')

defaultHostName=$(az functionapp show --name ${functionAppName} | jq -r '.defaultHostName')

functionsettings+=("HostUrl=https://${defaultHostName}")

echo "Configuring ${functionAppName} for https://${defaultHostName}"
az functionapp config appsettings set --name ${functionAppName} --resource-group ${rgname} --settings "${functionsettings[@]}"

echo "Deploying ${functionAppName} as zip"
./retry3.sh az functionapp deployment source config-zip -g ${rgname} -n ${functionAppName} --src "${packagePath}" --timeout 60


echo "Querying liveness endpoint for https://${defaultHostName}/.well-known/live"
./retry3.sh curl  -I -X GET --fail --show-error "https://${defaultHostName}/.well-known/live"