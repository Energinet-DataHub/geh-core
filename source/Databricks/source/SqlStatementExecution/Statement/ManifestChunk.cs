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

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;

// ReSharper disable InconsistentNaming
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable SA1300 // Element should begin with upper-case letter.

/// <summary>
/// <see cref="ManifestChunk"/> is used for mapping json results, hence we allow null values and inconsistent naming.
/// </summary>
internal class ManifestChunk
{
    public External_links[] external_links { get; set; }

    public class External_links
    {
        public long chunk_index { get; set; }

        public long row_offset { get; set; }

        public long row_count { get; set; }

        public long byte_count { get; set; }

        public string external_link { get; set; }

        public string expiration { get; set; }
    }
}

#pragma warning restore SA1300
#pragma warning restore CS8618
