# Serilog.Sinks.AzureQueueStorage [![Build status](https://ci.appveyor.com/api/projects/status/5hpbs9mkxye9blvk/branch/master?svg=true)](https://ci.appveyor.com/project/gjbdebug/serilog-sinks-azurequeuestorage/branch/master)   [![NuGet version](https://badge.fury.io/nu/Serilog.Sinks.AzureQueueStorage.svg)](https://badge.fury.io/nu/Serilog.Sinks.AzureQueueStorage)

Writes to a queue in [Windows Azure Queue Storage](https://docs.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues).

**Package** - Serilog.Sinks.AzureQueueStorage

**Modeled After** - [Serilog.Sinks.AzureTableStorage](http://nuget.org/packages/serilog.sinks.azuretablestorage)

**Example Use Case** - Log messages to an Azure storage queue and process asynchronously using an Azure Function.

```csharp
var storage = CloudStorageAccount.FromConfigurationSetting("MyStorage");

var log = new LoggerConfiguration()
    .WriteTo.AzureQueueStorage(storage)
    .CreateLogger();
```

### JSON configuration

It is possible to configure the sink using [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) by specifying the queue name and connection string in `appsettings.json`:

```json
"Serilog": {
  "WriteTo": [
    {"Name": "AzureQueueStorage", "Args": {"storageQueueName": "", "connectionString": ""}}
  ]
}
```

JSON configuration must be enabled using `ReadFrom.Configuration()`; see the [documentation of the JSON configuration package](https://github.com/serilog/serilog-settings-configuration) for details.

### XML `<appSettings>` configuration

To use the file sink with the [Serilog.Settings.AppSettings](https://github.com/serilog/serilog-settings-appsettings) package, first install that package if you haven't already done so:

```powershell
Install-Package Serilog.Settings.AppSettings
```

Instead of configuring the logger in code, call `ReadFrom.AppSettings()`:

```csharp
var log = new LoggerConfiguration()
    .ReadFrom.AppSettings()
    .CreateLogger();
```

In your application's `App.config` or `Web.config` file, specify the file sink assembly and required path format under the `<appSettings>` node:

```xml
<configuration>
  <appSettings>
    <add key="serilog:using:AzureQueueStorage" value="Serilog.Sinks.AzureQueueStorage" />
    <add key="serilog:write-to:AzureQueueStorage.connectionString" value="DefaultEndpointsProtocol=https;AccountName=ACCOUNT_NAME;AccountKey=KEY;EndpointSuffix=core.windows.net" />
    <add key="serilog:write-to:AzureQueueStorage.storageQueueName" value="YOUR_STORAGE_QUEUE_NAME" />
```
