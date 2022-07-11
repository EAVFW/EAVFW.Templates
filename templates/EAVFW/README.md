
# EAV Framework MainApp

## Quick start - with choker

1. `npm run gm`
2. `npm run build`
3. `npm run db-create`

Run it.

### Environment variables

```
# set the configuration for SMTP while we're at it
dotnet user-secrets set "Smtp:Port" "2500"
dotnet user-secrets set "Smtp:Host" "127.0.0.1"
dotnet user-secrets set "Smtp:EnableEmails" "true"
```

## Explained

### Scripts
Everything can be run with npm.

#### Build
Building everything with `npm build`

Build Framework with `npm build-framework`

Build Router with `npm build-router`


### Database

From scratch: `npm run db-create`

Already created: `npm run db-recreate`




################################### OLD ###################################
## Running locally

The app relies on an MS SQL database to run. Prior to running,

1. Install docker
2. Open a terminal in the project folder for EAVFW.Framework
3. Execute the following commands:

```cmd
# add connection string to user secrets
dotnet user-secrets set "ConnectionStrings:ApplicationDb" "Server=localhost; Initial Catalog=__databaseName__; User ID=sa; Password=Bigs3cRet"
```


```
# set the configuration for SMTP while we're at it
dotnet user-secrets set "Smtp:Port" "2500"
dotnet user-secrets set "Smtp:Host" "127.0.0.1"
dotnet user-secrets set "Smtp:EnableEmails" "true"
```

```
# generate initial migration script
dotnet test --filter "FullyQualifiedName=EAVFW.HelperScripts.DBDevSetup.InitializeDevDB" ../../scripts/__EAVFW__.HelperScripts/EAVFW.HelperScripts.csproj
dotnet test --filter "FullyQualifiedName=EAVFW.HelperScripts.DBDevSetup.InitializeSystemAdministrator" ../../scripts/__EAVFW__.HelperScripts/__EAVFW__.HelperScripts.csproj
```

```
# db setup
docker run -v ${PWD}/../../scripts/__EAVFW__.HelperScripts/bin/Debug/netcoreapp3.1/dbinit/:/opt/dbinit/ -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=Bigs3cRet' -e 'MSSQL_PID=Express' -p 1433:1433 --name __databaseName__ -d mcr.microsoft.com/mssql/server:2019-latest-ubuntu
docker exec -it __databaseName__ /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Bigs3cRet -Q "CREATE DATABASE __databaseName__"
docker exec -i __databaseName__ /opt/mssql-tools/bin/sqlcmd -s localhost -U sa -P Bigs3cRet -d __databaseName__ -i /opt/dbinit/init.sql -v DBSchema=__DATABASENAME__
docker exec -i __databaseName__ /opt/mssql-tools/bin/sqlcmd -s localhost -U sa -P Bigs3cRet -d __databaseName__ -i /opt/dbinit/init-systemadmin.sql -v DBName=__DATABASENAME__ -v DBSchema=__DATABASENAME__ -v UserGuid=1b714972-8d0a-4feb-b166-08d93c6ae329 -v UserName="Poul Kjeldager" -v UserEmail=pks@delegate.dk

```

You can then bring the database up and down with `docker start databaseName` and `docker stop databaseName` respectively.

To destroy your database and spawn a fresh one, do:

```
docker stop databaseName && docker rm databaseName
```

and then repeat the db setup procedure.

As a one-liner: 
```
docker stop databaseName; docker rm databaseName; docker run -v ${PWD}/../../scripts/EAVFW.HelperScripts/bin/Debug/netcoreapp3.1/dbinit/:/opt/dbinit/ -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=Bigs3cRet' -e 'MSSQL_PID=Express' -p 1433:1433 --name databaseName -d mcr.microsoft.com/mssql/server:2017-latest-ubuntu; dotnet user-secrets set "ConnectionString" "Server=127.0.0.1; Initial Catalog=DatabaseName; User ID=sa; Password=Bigs3cRet"; dotnet test --filter "ClassName=EAVFW.HelperScripts.DBDevSetup" ../../scripts/EAVFW.HelperScripts/EAVFW.HelperScripts.csproj; docker exec -it databaseName /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Bigs3cRet -Q "CREATE DATABASE DatabaseName"; docker exec -i databaseName /opt/mssql-tools/bin/sqlcmd -s localhost -U sa -P Bigs3cRet -d DatabaseName -I -i /opt/dbinit/init.sql -i /opt/dbinit/init-systemadmin.sql -v DBName=DATABASENAME -v DBSchema=PHCMS -v SystemAdminSecurityGroupId=1b714972-8d0a-4feb-b166-08d93c6ae328 -v UserGuid=1b714972-8d0a-4feb-b166-08d93c6ae329 -v UserName="Poul Kjeldager" -v UserEmail=pks@delegate.dk -v UserPrincipalName=PoulKjeldagerSørensen
```

### Frontend
Before running the projects, you should compile the Next.js apps.

For that, you will need to ensure you have the following installed:
* Node v.14
* npm version >= 7.0

The solution uses npm workspaces, so run `npm install` from the root of the project.
This will install all the necessary packages for all the front-end apps.

Create a `.env.local` file in `apps/EAVFW.ManagementPortal` and `apps/EAVFW.VirkDK` with the following contents :

```
NEXT_PUBLIC_API_BASE_URL=https://localhost:44306/api
NEXT_PUBLIC_BASE_URL=https://localhost:44306/
```

These settings may vary depending on the URL you are using to access the Router project. 
The default settings are on `https://localhost:44306` so if you haven't customized the hostname/port, you can copy it as-is.

With the .env.local files in place, run `npm run build` from both `apps/EAVFW.ManagementPortal` and `apps/EAVFW.VirkDK` folders.



## Updating Model Driven Framework
OBS - can only be run locally atm for those that has access to the framework. At the end of project, this is moved to its own nuget and target is added from nuget packages. Build step vill be the same and versioning controlled by nuget packages.
```
dotnet msbuild /target:CopyFramework
```

## Transpiling Manifest.json
Transpiling of manifest.json is done automatic on "Rebuild" target in msbuild, right click rebuild in visual studio or manually
```
dotnet msbuild /target:GenerateManifest
```

### QnA

*Question*: I can't build -- tons of classes are missing. What should I do?

*Answer*: It could be that the project failed to generate a manifest prior to the build step. Ensure the manifest was generated by executing the command:

```
pushd apps/EAVFW.ManagementPortal && dotnet msbuild /t:GenerateManifest && popd
```

*Question*: My tab with related entity is now showing up on form.

*Answer*: Verify that type lookup has the "forms" object pointing to the reference type and where it should add the element. Look for "genReference" log group to see if it finds the form.

# Testing emails

It is possible to test invitation and password recovery flows locally using a fake mail server.

Run the following command:

```
docker run -v ${PWD}/.config/mailslurper:/mailslurper -p 4436:4436 -p 4437:4437 -p 2500:2500 cycloid/mailslurper:1.14.1
```

You can then open the mailslurper inbox in your browser at http://127.0.0.1:4436/

Sending of emails is disabled by default in the `appsettings.local.json` and you will see  `OutgoingEmails` generated by plugins have status `EmailServiceDisabled`.

When you have mailslurper running locally, you can enable plugins from emails by adding the following user secret:

```
dotnet user-secrets set "Smtp:EnableEmails" "true"
```

# Authentication and Nemlogin

The Router is configured to use a mock Identity Provider for simulating Nemlogin when ran locally.
This mock automatically signs you in as the Svend Sørensen test user with no sign-in prompt.

You can run this mock IdP by executing the following command:

`dotnet run -p ./mocks/TestsIdPCore`

By default, the solution uses Nemlogin for signing into the end-user facing portion of the site.
The administrative portion (The Management Portal) uses Windows authentication (NTLM).

If you would prefer to use the Password-less Sign-in method for accessing the Management Portal instead of NTLM, add the following to user-secrets:

from `apps/EAVFW.ManagementPortal`:

```
dotnet user-secrets set "ReverseProxy:Routes:RouteForPortal:AuthorizationPolicy" "PasswordLess"
```


Combined with the mailslurper setup described above, this is a fast and easy way to authenticate.

# Testing adding new views

Start by defining a new view under "views" in manifest.json file. An example is:

...
"Districts Overview": {
    "type": "PowerBI",
    "workspaceId": "<workspace-id>",
    "reportId": "<report-id>",
    "tenantId": "<tenant-id>",
    "clientId": "<client-id>",
    "settings": {
        "filterPaneEnabled": false,
        "navContentPaneEnabled": false,
        "panes": {
            "pageNavigation": {
                "visible": false,
                "position": 1 //[@{PowerBI.PageNavigationPosition.Left}]
            }
        }
    }
},
...

Then a new view component should be created to show the newly added view. An example for powerBI.tsx:

...
import React from "react";

export type powerBIState = {
    type?: string;
    workspaceId?: string;
    view: any;
    data: any;
}

const PowerBIComponent: React.VFC = (props: any) => {
    const { view, data } = props;

    return (
        <div > 
          <div>  type: {view.type} </div>
          <div>  workspaceId: {view.workspaceId} </div>
          <div>  clientId: {view.clientId} </div>
        </div>
    );
};

export default PowerBIComponent;
...

The last step is to connect the component with the defined manifest view, and this is done by registering a new view in index.ts (found in CustomControls folder):

...
RegistereView("PowerBI", PowerBIComponent);
...


# End to End Automated UI testing
The entire project have been containerized to enable us to do E2E testing.

To run the docker, you must at a certificate at your `$USERPROFILE/.aspnet/https` named `aspnetapp.pfx` with the password `Bigs3cRet`.

This can be generated by running 

```powershell
dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\aspnetapp.pfx -p Bigs3cRet
dotnet dev-certs https --trust
```
or `$USERPROFILE` in bash.

For each change you make in one of either ManagementPortal or Router, you must publish the app. Navigate to one of the two and:
```
dotnet publish -c Debug
```

Now everything can be started by running (add `--build` if you want to force a rebuild.)
```
docker-compose up -d
```

NB: the database must be running in its own container for now.