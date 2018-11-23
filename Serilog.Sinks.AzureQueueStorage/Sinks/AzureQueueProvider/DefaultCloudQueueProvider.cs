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

namespace Serilog.Sinks.AzureQueueStorage.AzureQueueProvider
{
    class DefaultCloudQueueProvider : ICloudQueueProvider
    {
        readonly int _waitTimeoutMilliseconds = Timeout.Infinite;
        CloudQueue _cloudQueue;

        public CloudQueue GetCloudQueue(CloudStorageAccount storageAccount, string storageQueueName, bool bypassQueueCreationValidation)
        {
            if (_cloudQueue == null)
            {
                var cloudTableClient = storageAccount.CreateCloudQueueClient();
                _cloudQueue = cloudTableClient.GetQueueReference(storageQueueName);

                // In some cases (e.g.: SAS URI), we might not have enough permissions to create the queue if
                // it does not already exists. So, if we are in that case, we ignore the error as per bypassQueueCreationValidation.
                try
                {
                    _cloudQueue.CreateIfNotExistsAsync().SyncContextSafeWait(_waitTimeoutMilliseconds);
                }
                catch (Exception ex)
                {
                    Debugging.SelfLog.WriteLine($"Failed to create queue: {ex}");
                    if (!bypassQueueCreationValidation)
                    {
                        throw;
                    }
                }
            }
            return _cloudQueue;
        }
    }
}
