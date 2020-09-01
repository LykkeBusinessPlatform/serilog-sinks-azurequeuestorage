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

namespace Serilog.Sinks.AzureQueueStorage.AzureQueueProvider
{
    class DefaultCloudQueueProvider : ICloudQueueProvider
    {
        readonly int _waitTimeoutMilliseconds = Timeout.Infinite;

        public CloudQueue GetCloudQueue(CloudStorageAccount storageAccount, string storageQueueName, bool bypassQueueCreationValidation)
        {
            var cloudQueueClient = storageAccount.CreateCloudQueueClient();
            var cloudQueue = cloudQueueClient.GetQueueReference(storageQueueName);

            // In some cases (e.g.: SAS URI), we might not have enough permissions to create the queue if
            // it does not already exists. So, if we are in that case, we ignore the error as per bypassQueueCreationValidation.
            try
            {
                cloudQueue.CreateIfNotExistsAsync().SyncContextSafeWait(_waitTimeoutMilliseconds);
            }
            catch (Exception ex)
            {
                Debugging.SelfLog.WriteLine($"Failed to create queue: {ex}");
                if (!bypassQueueCreationValidation)
                    throw;
            }
            return cloudQueue;
        }
    }
}
