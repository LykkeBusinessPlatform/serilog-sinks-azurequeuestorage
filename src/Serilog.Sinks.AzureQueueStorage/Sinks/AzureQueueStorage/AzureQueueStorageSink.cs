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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.AzureQueueStorage.AzureQueueProvider;

namespace Serilog.Sinks.AzureQueueStorage
{
    /// <summary>
    /// Writes log events as records to an Azure Queue Storage queue.
    /// </summary>
    public class AzureQueueStorageSink : ILogEventSink
    {
        private readonly int _waitTimeoutMilliseconds = Timeout.Infinite;
        private readonly ITextFormatter _textFormatter;
        private readonly CloudStorageAccount _storageAccount;
        private readonly string _storageQueueName;
        private readonly bool _bypassQueueCreationValidation;
        private readonly bool _separateQueuesByLogLevel;
        private readonly ICloudQueueProvider _cloudQueueProvider;
        private readonly CloudQueue _queue;
        private readonly ConcurrentDictionary<LogEventLevel, CloudQueue> _queuesDictionary;

        /// <summary>
        /// Construct a sink that saves logs to the specified storage account.
        /// </summary>
        /// <param name="storageAccount">The Cloud Storage Account containing the queue.</param>
        /// <param name="textFormatter"></param>
        /// <param name="storageQueueName">Queue name that messages will be written to.</param>
        /// <param name="bypassQueueCreationValidation">Bypass the exception in case the queue creation fails.</param>
        /// <param name="separateQueuesByLogLevel">Flag for several queues usage by log level</param>
        /// <param name="cloudQueueProvider">Cloud queue provider to get current queue.</param>
        public AzureQueueStorageSink(
            CloudStorageAccount storageAccount,
            ITextFormatter textFormatter,
            string storageQueueName = null,
            bool bypassQueueCreationValidation = false,
            bool separateQueuesByLogLevel = false,
            ICloudQueueProvider cloudQueueProvider = null)
        {
            _textFormatter = textFormatter;
            _storageAccount = storageAccount;
            _storageQueueName = storageQueueName;
            _bypassQueueCreationValidation = bypassQueueCreationValidation;
            _separateQueuesByLogLevel = separateQueuesByLogLevel;
            _cloudQueueProvider = cloudQueueProvider ?? new DefaultCloudQueueProvider();
            if (separateQueuesByLogLevel)
                _queuesDictionary = new ConcurrentDictionary<LogEventLevel, CloudQueue>();
            else
                _queue = _cloudQueueProvider.GetCloudQueue(_storageAccount, _storageQueueName, _bypassQueueCreationValidation);
        }

        /// <summary>
        /// Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write.</param>
        public void Emit(LogEvent logEvent)
        {
            TextWriter writer = new StringWriter();
            _textFormatter.Format(logEvent, writer);
            var output = writer.ToString();

            CloudQueueMessage message = new CloudQueueMessage(output);

            var queue = GetQueue(logEvent.Level, logEvent.Properties);

            queue.AddMessageAsync(message)
                .SyncContextSafeWait(_waitTimeoutMilliseconds);
        }

        private CloudQueue GetQueue(LogEventLevel level, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            if (!_separateQueuesByLogLevel)
                return _queue;

            if (_queuesDictionary.TryGetValue(level, out var queue))
                return queue;

            queue = _cloudQueueProvider.GetCloudQueue(
                _storageAccount,
                $"{_storageQueueName}-{GetLogLevelSuffix(level, properties)}",
                _bypassQueueCreationValidation);

            _queuesDictionary.TryAdd(level, queue);

            return queue;
        }

        private string GetLogLevelSuffix(LogEventLevel level, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            switch (level)
            {
                case LogEventLevel.Verbose:
                    return "trace";
                case LogEventLevel.Debug:
                    return "debug";
                case LogEventLevel.Information:
                    return "information";
                case LogEventLevel.Warning:
                    if (properties.ContainsKey("Monitor"))
                        return "monitor";
                    return "warning";
                case LogEventLevel.Error:
                    return "error";
                case LogEventLevel.Fatal:
                    return "critical";
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}

