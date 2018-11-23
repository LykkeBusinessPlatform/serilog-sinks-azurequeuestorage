Serilog.Sinks.AzureQueueStorage

            [![Build Status](https://dev.azure.com/sector7g/Serilog.Sinks.AzureQueueStorage/_apis/build/status/Serilog.Sinks.AzureQueueStorage-.NET%20Desktop-CI)](https://dev.azure.com/sector7g/Serilog.Sinks.AzureQueueStorage/_build/latest?definitionId=12)

            Implementation of an Azure storage queue sink based off the Azure table storage sink:
            https://github.com/serilog/serilog-sinks-azuretablestorage

            Certain functionality/code existing in the Azure table sink removed in this Azure queue sink
            should be reviewed and some may need to be restored.
            
            Tests definitely need to be reinstated.
            (this was built in less than a day to address an urgent need).
            
            Target frameworks: .NET 4.6.1 & .NET Core 2.1
            
            Usage:
            
            Usage follows Serilog.Sinks.AzureTableStorage except:
            - "WriteTo" is AzureQueueStorage instead of AzureTableStorage
            - "storageTableName" is "storageQueueName" (and use an existing queue's name as a parameter)
            
            "connectionString" is still a valid storage account connection string, in this case where the queue is defined.