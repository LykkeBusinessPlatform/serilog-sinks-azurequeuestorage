// Copyright 2018 Sector 7G Communications
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.AzureQueueStorage.AzureQueueProvider;

using Newtonsoft.Json;

namespace Serilog.Sinks.AzureQueueStorage
{
    /// <summary>
    /// Writes log events as records to an Azure Queue Storage table.
    /// </summary>
    public class AzureQueueStorageSink : ILogEventSink
    {
        readonly int _waitTimeoutMilliseconds = Timeout.Infinite;
        readonly ITextFormatter _textFormatter;
        readonly CloudStorageAccount _storageAccount;
        readonly string _storageQueueName;
        readonly bool _bypassQueueCreationValidation;
        readonly ICloudQueueProvider _cloudQueueProvider;

        /// <summary>
        /// Construct a sink that saves logs to the specified storage account.
        /// </summary>
        /// <param name="storageAccount">The Cloud Storage Account to use to insert the log entries to.</param>
        /// <param name="textFormatter"></param>
        /// <param name="storageQueueName">Queue name that log entries will be written to.</param>
        /// <param name="keyGenerator">generator used to generate partition keys and row keys</param>
        /// <param name="bypassQueueCreationValidation">Bypass the exception in case the table creation fails.</param>
        /// <param name="cloudQueueProvider">Cloud queue provider to get current log table.</param>
        public AzureQueueStorageSink(
            CloudStorageAccount storageAccount,
            ITextFormatter textFormatter,
            string storageQueueName = null,
            bool bypassQueueCreationValidation = false,
            ICloudQueueProvider cloudQueueProvider = null)
        {
            _textFormatter = textFormatter;
            _storageAccount = storageAccount;
            _storageQueueName = storageQueueName;
            _bypassQueueCreationValidation = bypassQueueCreationValidation;
            _cloudQueueProvider = cloudQueueProvider ?? new DefaultCloudQueueProvider();
        }

        /// <summary>
        /// Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write.</param>
        public void Emit(LogEvent logEvent)
        {
            var queue = _cloudQueueProvider.GetCloudQueue(_storageAccount, _storageQueueName, _bypassQueueCreationValidation);

            CloudQueueClient queueClient = _storageAccount.CreateCloudQueueClient();
            CloudQueue storageQueueName = queueClient.GetQueueReference(_storageQueueName);
            CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(logEvent));

            queue.AddMessageAsync(message)
                .SyncContextSafeWait(_waitTimeoutMilliseconds);
        }
    }
}

