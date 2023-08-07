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

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecutionTests.Assets;

public class TestFiles
{
    public string TimeSeriesResponse => GetFileAsString("time_series_response.json");

    private string GetFileAsString(string fileName)
    {
        var stream = GetFileStream(fileName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private Stream GetFileStream(string fileName)
    {
        var rootNamespace = GetType().Namespace!;
        return GetType().Assembly.GetManifestResourceStream($"{rootNamespace}.{fileName}");
    }
}