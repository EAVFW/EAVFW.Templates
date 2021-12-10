# EAVFW.ExpressionEngine.Blazor

This project uses EAVFW.ExpressionEngine to parse expressions and makes them available as a WebAssembly to JavaScript.

## Copy the framework
Inorder to use the packaged webassembly, the output have to be copied from this project to the static folder of your web app. This is all done by including the `expressionEngineBlazor.targets` file in your project.

It uses the following proeprties:
* `ExpressionEngineBlazorPath`: Relative/full path to this project (**Mandatory**)
* `ExpressionEngineBlazorDestinationPath`: Relative/full destination path (Optional)


### Example
```xml
...

<Import Project="..\..\src\EAVFW.ExpressionEngine.Blazor\build\expressionEngineBlazor.targets" />

<PropertyGroup>
    ...
    <ExpressionEngineBlazorDestinationPath>$(MSBuildProjectDirectory)\wwwroot</ExpressionEngineBlazorDestinationPath>
    <ExpressionEngineBlazorPath>..\..\src/EAVFW.ExpressionEngine.Blazor</ExpressionEngineBlazorPath>
</PropertyGroup>

...
````


## Load framework
Include this script statement, to load the webassembly.
```js
<script src="/<path>/blazor.webassembly.js" />
```