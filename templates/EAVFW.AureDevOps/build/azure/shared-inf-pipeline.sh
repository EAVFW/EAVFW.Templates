#!/bin/bash
set -Eeuxo pipefail
 

source pipeline-setup.sh;


echo "Creating SQL Server"

if [ "${projectEnv,,}" == "prod" ]; then

sqlservername=$(echo "${prefixShort}-${sqlprefix}-${projectName}-${locationShort}-${projectEnv}" | sed 's/[- 0-9]*$//' | sed 's/[- 0-9]*//' | awk '{print tolower($0)}')
sqldbname=$(echo "${prefixShort}-${sqldbprefix}-${projectName}-${locationShort}-${projectEnv}" | sed 's/[- 0-9]*$//' | sed 's/[- 0-9]*//' | awk '{print tolower($0)}')

else

sqlservername=$(echo "${prefixShort}-${sqlprefix}-${projectName}-${locationShort}-dev" | sed 's/[- 0-9]*$//' | sed 's/[- 0-9]*//' | awk '{print tolower($0)}')
sqldbname=$(echo "${prefixShort}-${sqldbprefix}-${projectName}-${locationShort}-dev" | sed 's/[- 0-9]*$//' | sed 's/[- 0-9]*//' | awk '{print tolower($0)}')

fi

sqlservernameLength=$(expr length "${sqlservername}")

if [ ${sqlservernameLength} -gt 62 ]
then
	COUNT=`expr ${sqlservernameLength} - 62`
	sqlservernameshort=$(echo "${projectName}" | rev | cut -c ${COUNT}- | rev)
	sqlservername=$(echo "${prefixShort}-${sqlprefix}-${sqlservernameshort}-${locationShort}-${projectEnv}" | sed 's/[- 0-9]*$//' | sed 's/[- 0-9]*//' | awk '{print tolower($0)}')
fi


doServerExist=$(az sql server list --query "[?name == '$sqlservername']")

#Create Azure SQL Server  (dev is used for dev,uat,test)


if [[ "${doServerExist}" = "[]" ]]; then
	dbpass=$(openssl rand -base64 64)
	az keyvault secret set --name sql-$sqladminusername --vault-name ${keyVaultName} --value "$dbpass"

	az sql server create --admin-password "$dbpass" --admin-user $sqladminusername --name $sqlservername
    az sql server firewall-rule create -s $sqlservername -n AzureIP --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
    #
    #azureaduser=$(az ad user list --filter "userPrincipalName eq 'KONS-PKS@dlf.org'" --query [].objectId --output tsv)
    az sql server ad-admin create --server-name $sqlservername --display-name ${dbinituser} --object-id "${dbinituserid}"
fi

echo "Creating Database"
#az sql db create -s $sqlservername -n $sqldbname -e GeneralPurpose -f Gen5 -c 0.5 --compute-model Serverless --auto-pause-delay 60
az sql db create -s $sqlservername -n $sqldbname --service-objective S0

echo "Creating AppService Plan"
az appservice plan create -n $appplanname --sku P1V2



storageAccountName="stgadir"

echo "Create Storage Account (all lower case)"
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

echo "Creating WebApp" 
az webapp create  -p $appplanname -n ${AppName} --assign-identity [system]
 

echo "Configure SQL DB" 
sqlinfo=$(az sql server show --name $sqlservername)
#sqlcmd -d ${sqldbname} -S $(echo "$sqlinfo" | jq -r '.fullyQualifiedDomainName') -U $sqladminusername -P "$(az keyvault secret show --name sql-$sqladminusername --vault-name ${keyVaultName} --query 'value' -o tsv)" -v WebAppName="${AppName}" -i sqlinit.sql 
sqlserverdomainname=$(echo "$sqlinfo" | jq -r '.fullyQualifiedDomainName')
#sqlcmd -d ${sqldbname} -S ${sqlserverdomainname} -G -U ${dbinituser} -P "${dbinitpassword}" -v WebAppName="${AppName}" -v schema="dlf_${projectEnv}"  -i sqlinit.sql
#sqlcmd -d ${sqldbname} -S ${sqlserverdomainname} -G -U ${dbinituser} -P "${dbinitpassword}" -v schema="dlf_${projectEnv}" -i db-migrations.sql
sqlcmd -d ${sqldbname} -S ${sqlserverdomainname} -G -U ${dbinituser} -P "${dbinitpassword}" -I -v DBName=${sqldbname} -v DBSchema="__EAVFW___${projectEnv}" -v SystemAdminSecurityGroupId=1b714972-8d0a-4feb-b166-08d93c6ae328 -v UserGuid=1b714972-8d0a-4feb-b166-08d93c6ae329 -v UserName="Poul Kjeldager" -v UserEmail=pks@delegate.dk -i init.sql -i init-systemadmin.sql

defaultHostName=$(az webapp show --name ${AppName} | jq -r '.defaultHostName')

if [ "${projectEnv,,}" == "prod" ] || [ "${projectEnv,,}" == "uat" ] ; then
SyncIntraNoteMaskEmail="false"
else
SyncIntraNoteMaskEmail="true"
fi
appsettings=(
"APPINSIGHTS_INSTRUMENTATIONKEY=${instrumentationKey}"
"APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=${instrumentationKey}"
"ApplicationInsightsAgent_EXTENSION_VERSION=~2"
"ConnectionString=Data Source=${sqlserverdomainname};Initial Catalog=${sqldbname};Authentication=Active Directory Managed Identity"
"HostUrl=https://${defaultHostName}"
"DBSchema=__EAVFW___${projectEnv}"
"Storage=${local_current_env_conn_string}"
"SENDGRID_REPLY_ADDRESS=${SENDGRID_REPLY_ADDRESS}"
"SENDGRID_APIKEY=${SENDGRID_APIKEY}"
)


echo "Assigning Access for ${AppName}"
az keyvault set-policy --name ${keyVaultName} --secret-permissions get --object-id $(az webapp show --name ${AppName} | jq -r '.identity.principalId')


echo "Configuring ${AppName} for https://${defaultHostName}"
az webapp config appsettings set --name ${AppName}  --settings "${appsettings[@]}"
