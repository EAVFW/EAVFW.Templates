# EAVFW

Cool get started ReadMe

## Install framework
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

```
dotnet new eavfw
```

or 

```
dotnet new eavfw --projectPrefix "<customerAcronym>" --mainAppReplace "<myApp>" --databaseName "<mainDatabase1>"
```

Example
dotnet new eavfw --projectPrefix "MyOrg001" --mainAppReplace "MyEAVTestApp001" --databaseName "MyEAVTestAppDatabase001"


## Overall guide for starting a new EAV project from this Template

https://github.com/EAVFW/EAVFW/wiki