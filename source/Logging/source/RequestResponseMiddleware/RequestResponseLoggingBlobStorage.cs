﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Energinet.DataHub.Core.Logging.RequestResponseMiddleware
{
    public class RequestResponseLoggingBlobStorage : IRequestResponseLogging
    {
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;

        public RequestResponseLoggingBlobStorage(
            string storageConnectionString,
            string storageContainerName)
        {
            _storageConnectionString = storageConnectionString;
            _storageContainerName = storageContainerName;
        }

        public async Task LogRequestAsync(Stream logStream, Dictionary<string, string> metaData, Dictionary<string, string> indexTags, string logName, string folder)
        {
            var nameWithFolder = $"{folder}/{logName}";
            var blobClientRequest = new BlobClient(_storageConnectionString, _storageContainerName, nameWithFolder);
            var options = new BlobUploadOptions { Tags = indexTags, Metadata = metaData };
            await blobClientRequest.UploadAsync(logStream, options);
        }

        public async Task LogResponseAsync(Stream logStream, Dictionary<string, string> metaData, Dictionary<string, string> indexTags, string logName, string folder)
        {
            var nameWithFolder = $"{folder}/{logName}";
            var blobClientResponse = new BlobClient(_storageConnectionString, _storageContainerName, nameWithFolder);
            var options = new BlobUploadOptions { Tags = indexTags, Metadata = metaData };
            await blobClientResponse.UploadAsync(logStream, options);
        }
    }
}
