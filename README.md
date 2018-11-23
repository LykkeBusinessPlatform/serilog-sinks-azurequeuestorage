Serilog.Sinks.AzureQueueStorage

            Implementation of an Azure storage queue sink based off the Azure table storage sink:
            https://github.com/serilog/serilog-sinks-azuretablestorage

            Certain functionality/code existing in the Azure table sink removed in this Azure queue sink
            should be reviewed and some may need to be restored.
            (this was built in less than a day to address an urgent need).
            
            Target frameworks: .NET 4.6.1 & .NET Core 2.1
            
            Usage:
            
            Usage follows Serilog.Sinks.AzureTableStorage except:
            - "WriteTo" is AzureQueueStorage instead of AzureTableStorage
            - "storageTableName" is "storageQueueName" (and use an existing queue's name as a parameter)
            
            "connectionString" is still a valid storage account connection string, in this case where the queue is defined.