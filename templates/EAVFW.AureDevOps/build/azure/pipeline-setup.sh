#!/bin/bash
set -Eeuxo pipefail

prefix=$(echo "$prefix" | awk '{$1=$1};1')

#Set Prefixes used for resources
prefixShort=$(echo "$prefix" | cut -c -3)
kvprifix="KeyV"
sqlpoolprefix="sqlpool"
sqlprefix="sql"
sqldbprefix="sqldb"
eventgridprefix="eg-domain"

az extension add --name application-insights

 

EnvProjectName=$(echo "${projectName}-${locationShort}-${projectEnv}"  | sed 's/[- 0-9]*$//' | sed 's/[- 0-9]*//')



#The ResourceGroup Name
rgname=$(echo "${RGName}") #$(echo  "rg-${EnvProjectName}" | sed 's/[- 0-9]*$//' | sed 's/[- 0-9]*//' | awk '{print tolower($0)}')

#Set default location and ResourceGroup name
az configure --defaults location=${location} group=${rgname}

#Create resource group if not exists
#rgid=$(az group create -g ${rgname} | jq -r '.id')

#Create App Insights for monitoring
appInsightsName="AI-${EnvProjectName}"
instrumentationKey=$(az monitor app-insights component create --app "${appInsightsName}" | jq -r '.instrumentationKey')

#Create keyVault and trim projectname to be shorter then 24 length name
keyVaultName=$(echo "${prefixShort}-${kvprifix}-${projectName}-${locationShort}-${projectEnv}" | sed 's/[- 0-9]*$//' | sed 's/[- 0-9]*//' | awk '{print tolower($0)}')
keyVaultNameLength=$(expr length "${keyVaultName}")

if [ ${keyVaultNameLength} -gt 23 ]
then
    COUNT=`expr ${keyVaultNameLength} - 23`
	kvnshort=$(echo "${projectName}" | rev | cut -c ${COUNT}- | rev)
	keyVaultName=$(echo "${prefixShort}-${kvprifix}-${kvnshort}-${locationShort}-${projectEnv}" | sed 's/[- 0-9]*$//' | sed 's/[- 0-9]*//' | awk '{print tolower($0)}')
fi

isKeyVaultDeleted=$(az keyvault list-deleted --query "[?name == '$keyVaultName']")
if [[ "${isKeyVaultDeleted}" != "[]" ]]; then
az keyvault purge --name $keyVaultName
fi

doKeyVaultExist=$(az keyvault list --query "[?name == '$keyVaultName']")

if [[ "${doKeyVaultExist}" = "[]" ]]; then
keyVaultId=$(az keyvault create \
	--name ${keyVaultName} \
	--enable-soft-delete true \
	--retention-days 7 \
	--enabled-for-template-deployment | jq -r '.id')
fi

#Create Storage Account (all lower case)
accountName=$(echo "${prefixShort}${projectName}${locationShort}${projectEnv}" | sed 's/-//g' | awk '{print tolower($0)}')
accountNameLength=$(expr length "${accountName}")

if [ ${accountNameLength} -gt 23 ]
then
    COUNT=`expr ${accountNameLength} - 23`
	acnshort=$(echo "${projectName}" | rev | cut -c ${COUNT}- | rev)
	accountName=$(echo "${prefixShort}${acnshort}${locationShort}${projectEnv}" | sed 's/-//g' | awk '{print tolower($0)}')
fi


az storage account create -n ${accountName} --sku Standard_LRS --kind StorageV2

#SetOrGet the storage key into a secret named AzureWebJobsStorage
dosecretexist=$(az keyvault secret list --vault-name ${keyVaultName} --query "[?name == 'storage-${accountName}']")
current_env_conn_string=$(az storage account show-connection-string -n ${accountName} --query 'connectionString' -o tsv)

if [[ "${dosecretexist}" == "[]" ]]; then
	secretUriWithVersion=$(az keyvault secret set --name storage-${accountName} --vault-name ${keyVaultName} --value ${current_env_conn_string} | jq -r '.id')
else
	current_account_fromvault_conn_string=$(az keyvault secret show --name storage-${accountName} --vault-name ${keyVaultName} --query 'value' -o tsv)
	

	if [ "$current_account_fromvault_conn_string" = "$current_env_conn_string" ]; then
	secretUriWithVersion=$(az keyvault secret show --name storage-${accountName} --vault-name ${keyVaultName} --query 'id' -o tsv)
	else
	secretUriWithVersion=$(az keyvault secret set --name storage-${accountName} --vault-name ${keyVaultName} --value ${current_env_conn_string} | jq -r '.id')
	fi
fi


if [ "${projectEnv,,}" == "prod" ]; then
appplanname=$(echo  "AppPlan-${projectName}-${locationShort}-prod")
else
appplanname=$(echo  "AppPlan-${projectName}-${locationShort}-dev")
fi

AppName="${prefixShort}-WebApp-${projectName}-${locationShort}-${projectEnv}"

