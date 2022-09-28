#!/bin/bash
set -Eeuxo pipefail

projectName=$1
projectEnv=$2
location=$3
prefix=$4

az extension add --name application-insights

EnvProjectName=$(echo "${projectName}-${projectEnv}" | sed 's/[- 0-9]*$//')

#The ResourceGroup Name
rgname="RG-${EnvProjectName}"

#Create resource group if not exists
az group create \
	--location ${location} \
    -g ${rgname}

#Create App Insights for monitoring
appInsightsName="AI-${EnvProjectName}"
instrumentationKey=$(az monitor app-insights component create --app "${appInsightsName}" --location ${location} --resource-group ${rgname} | jq -r '.instrumentationKey')


#Create Storage Account (all lower case)
accountName=$(echo "${prefix}${EnvProjectName}" | sed 's/-//g' | awk '{print tolower($0)}')
az storage account create \
	-n ${accountName} \
	-g ${rgname} \
	-l ${location} \
	--sku Standard_LRS \
	--kind StorageV2

#Create keyVault and trim projectname to be shorter then 24 length name
keyVaultName=${prefix}KeyV${projectName}${projectEnv}
keyVaultNameLength=$(expr length "${keyVaultName}")

if [ ${keyVaultNameLength} -gt 23 ]
then
    COUNT=`expr ${keyVaultNameLength} - 23`
	kvnshort=$(echo "${projectName}" | rev | cut -c ${COUNT}- | rev)
	keyVaultName=${prefix}KeyV${kvnshort}${projectEnv}
fi


keyVaultId=$(az keyvault create \
	--name ${keyVaultName} \
	--resource-group ${rgname} \
	--location ${location} \
	--enable-soft-delete false \
	--enabled-for-template-deployment | jq -r '.id')

#Set the storage key into a secret named AzureWebJobsStorage
#keysJson=$(az storage account keys list -g ${rgname} -n ${accountName})
current_env_conn_string=$(az storage account show-connection-string -g ${rgname} -n ${accountName} --query 'connectionString' -o tsv)
secretUriWithVersion=$(az keyvault secret set --name AzureWebJobsStorage --vault-name ${keyVaultName} --value ${current_env_conn_string} | jq -r '.id')


#Deploy Function
#name="Ingestor"
#functionAppName=$(echo "${prefix}-WebApp-${projectName}${name}-${projectEnv}" | sed 's/[- 0-9]*$//')
#$publishFolder = "FunctionsDemo/bin/Release/netcoreapp2.1/publish"

#Create FunctionApp
#az functionapp create --functions-version 3 -n ${functionAppName} --storage-account ${accountName} --consumption-plan-location ${location} --app-insights-key ${instrumentationKey} --runtime dotnet -g ${rgname}

#configure function
#az functionapp config appsettings set --name ${functionAppName} --resource-group ${rgname} --settings 

# deploy the zipped package
#az functionapp deployment source config-zip -g ${rgname} -n ${functionAppName} --src ${publishFolder}