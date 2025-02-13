[![NuGet](https://img.shields.io/nuget/v/Serilog.Sinks.LogtailSysBlazor.svg)](https://www.nuget.org/packages/Serilog.Sinks.LogtailSysBlazor)

A [Serilog](https://serilog.net) sink that logs events to logtail.

### Getting started

This project is a fork of : [https://github.com/Nickztar/Serilog.Sinks.LogtailSys](https://www.nuget.org/packages/Serilog.Sinks.LogtailSysBlazor)

Install the [Serilog.Sinks.LogtailSysBlazor](https://www.nuget.org/packages/Serilog.Sinks.LogtailSysBlazor) package from NuGet:

Using Package Manager
In Visual Studio, open **NuGet Package Manager Console** by clicking **Tools** → **NuGet Package Manager** → **Package Manager Console**

In the console run the following command:
```powershell
Install-Package Serilog.Sinks.LogtailSysBlazor
```

Optionally using Dotnet CLI
```powershell
dotnet add package Serilog.Sinks.LogtailSysBlazor
```

To start logging you need to setup a rsyslog source in Logtail, please use their documentation for this.
[Logtail - Getting started](https://betterstack.com/docs/logs/logging-start/)


To configure the sink to write messages to logtail, call `WriteTo.Logtail()` with your logtail token during logger configuration:

```csharp
var log = new LoggerConfiguration()
    .WriteTo.Logtail(token: "$SOURCE_TOKEN")
    .CreateLogger();
```


A number of optional parameters are available for more advanced configurations, see doc tags for more information.


NOTE: Project is in no way associated with Logtail or Serilog
