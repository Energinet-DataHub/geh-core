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

using System.Net.Http;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Databricks;

/// <summary>
/// Factory to create http client
/// </summary>
public interface IHttpClientFactory
{
    /// <summary>
    /// creat a client with the given settings
    /// </summary>
    /// <param name="databricksSettings"></param>
    /// <returns>http client</returns>
    HttpClient CreateHttpClient(DatabricksSettings databricksSettings);
}
