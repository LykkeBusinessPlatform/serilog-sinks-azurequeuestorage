// ================================================================================
// Serilog.Sinks.AzureQueueStorage
// 
// MIT License
//
// Copyright 2018-2019 Gregory J. Butler
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ================================================================================
//
// See NOTICES accompanying this package for any third party attribution.
//

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
    /// Writes log events as records to an Azure Queue Storage queue.
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
        /// <param name="storageAccount">The Cloud Storage Account containing the queue.</param>
        /// <param name="textFormatter"></param>
        /// <param name="storageQueueName">Queue name that messages will be written to.</param>
        /// <param name="keyGenerator">generator used to generate partition keys and row keys</param>
        /// <param name="bypassQueueCreationValidation">Bypass the exception in case the queue creation fails.</param>
        /// <param name="cloudQueueProvider">Cloud queue provider to get current queue.</param>
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

