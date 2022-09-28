# EAVFW

Cool get started ReadMe

## Install framework

From the root of the project run:
```
dotnet new --install .\templates\EAVFW\
```
then also run:
```
dotnet new --install .\templates\EAVFW.NextJS\
``` 
## Uninstall framework (to reinstall it)
```
dotnet new --uninstall .\templates\EAVFW\
```


## Apply the template in another folder
At a folder on the same level as the template folder, run:

```
dotnet new eavfw
```

or 

```
dotnet new eavfw --namespace "<customerAcronym>" --appName "<myApp>" --databaseName "<mainDatabase1>"
```

where 
- ```<customerAcronym>``` could be an acrynom for the customer organisation. 
- ```<myApp>``` could be the name of the app. 
- ```<mainDatabase1>``` could be the desired name for the database schema.

Example:
dotnet new eavfw --namespace "MyOrg001" --appName "MyEAVTestApp001" --databaseName "MyEAVTestAppDatabase001"

<br> 
For information about the parameters, run: 
```
dotnet new eavfw -h
```

## Overall guide for starting a new EAV project from this Template
https://github.com/EAVFW/EAVFW/wiki


## Guide for setting up base project
(*step 1 to 3 is the same as above section _Apply the template in another folder_)
- Step 1 Make a new directory at the same level as the cloned EAVFW.Templates. This directory will become the root directory for the generated EAV project. (e.g. a new directory called "MyEAV001" located in the "dev" directory)
- Step 2: Open a CLI in the root of the new directory.
- Step 3: Start the new project by using ```dotnet new eavfw --namespace "<customerAcronym>" --appName "<MyApp>" --databaseName "<DatabaseName>"``` . Where:
    - `<customerAcronym>` represents an acronym or short name for the customer for which the solution will be developed. 
    - `<MyApp>` represents the name of the app project in the solution.
    - `<DatabaseName>` represents the name of the main database schema.
(e.g. ```dotnet new eavfw --namespace "MyOrg001" --appName "MyEAVTestApp001" --databaseName "MyEAVTestAppDatabase001"```)
- Step 4: When applying the framework the CLI output will request confirmation for the usage of different tools and extensions. You most likely want to accept all of them (`y` and hit enter when prompted).
- Step 5: Open the project in an IDE (can be done simply be opening the .csproj file, from the newly created directory containing the solution, in VS).
- Step 6: After the project has completed opening. Rebuild the solution twice. Then build the solution twice.


## Guide for setting up for web
- Step 1: Open a CLI in the root of the repository created in _Guide for setting up base project_
- Step 2: Generate the web files using ```npm run eav-nextjs``` .
- Step 3: Install the needed dependencies using ```npm install --force```
- Step 3.5: If not already done as part of applying the template, run ```npm run gm``` to generate the manifest.
- Step 4: Navigate to the web portal in the CLI. Can be done using ```cd .\apps\<customerAcronym>.<MyApp>``` where <customerAcronym> and <MyApp> is replaced by the names you chose in part 1 (you can click tab in most CLIs to get suggestions).
- Step 5: Build the web project with ```npm run build```. This should build and export the files required for the web portal to run.


## Guide for creating and applying database migration

- Step 1: `dotnet tool run eavfw-manifest manifest new-migration` before any database changes are done to the manifest(s).
- Step 2: __Make cahnges tot he manifest__ and bump the version in `manifest.json`
- Step 3: `npm run gm` to generate the manifest with the changes.
- Step 4: `npm run eavfw-gen-migrations` to generate the migration script. (They are located in `/obj/dbinit` in the root of the project)
- Step 5: `npm run eavfw-apply-migrations` to apply the migration.

Step 3 to 5 can be executed running `npm run eavfw-migrate`.

## When developing
If changes are made to the template it can be necessary to refresh cache for the cmd window or terminal used to apply the template:
```
dotnet new --debug:rebuildcache
```

## Developing on the newly created project
See documentation in the newly created projects README located in the 'Solution Items' folder. Or go to https://github.com/EAVFW/EAVFW/wiki.