// Copyright 2020 Energinet DataHub A/S
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
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Storage;

namespace RequestResponseMiddleware.Tests
{
    public class LocalLogStorage : IRequestResponseLogging
    {
        private readonly List<LocalLog> _logs = new();

        public async Task LogRequestAsync(Stream logStream, Dictionary<string, string> metaData, Dictionary<string, string> indexTags, string logName)
        {
            await SaveLogAsync(logStream, metaData, logName);
        }

        public async Task LogResponseAsync(Stream logStream, Dictionary<string, string> metaData, Dictionary<string, string> indexTags, string logName)
        {
            await SaveLogAsync(logStream, metaData, logName);
        }

        public IEnumerable<LocalLog> GetLogs()
        {
            return _logs;
        }

        private async Task SaveLogAsync(Stream logStream, Dictionary<string, string> metaData, string logName)
        {
            // We use ASCII to detect odd encoding conversions in UTs.
            var reader = new StreamReader(logStream, Encoding.ASCII, false);
            var logMsg = await reader.ReadToEndAsync();
            _logs.Add(new LocalLog() { Body = logMsg, MetaData = metaData, LogName = logName });
        }
    }
}
