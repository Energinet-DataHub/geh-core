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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

/// <summary>
/// Represents options for configuring Databricks SQL Warehouse queries.
/// </summary>
public class QueryOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryOptions"/> class.
    /// </summary>
    /// <param name="format">The format in which the results should be returned.</param>
    /// <param name="downloadInParallel">Indicates whether to download results in parallel.</param>
    private QueryOptions(Format format, bool downloadInParallel)
    {
        Format = format;
        DownloadInParallel = downloadInParallel;
    }

    /// <summary>
    /// Gets the format in which the results should be returned.
    /// </summary>
    public Format Format { get; private set; }

    /// <summary>
    /// Gets the default query options with Apache Arrow format and no parallel download.
    /// </summary>
    public static QueryOptions Default => new(Format.ApacheArrow, false);

    /// <summary>
    /// Gets the query options with Apache Arrow format and parallel download.
    /// </summary>
    public static QueryOptions ApacheArrowParallel => new(Format.ApacheArrow, true);

    /// <summary>
    /// Creates a new instance of <see cref="QueryOptions"/> with the specified format.
    /// </summary>
    /// <param name="format">The format in which the results should be returned.</param>
    /// <returns>A new instance of <see cref="QueryOptions"/>.</returns>
    public static QueryOptions WithFormat(Format format) => new(format, false);

    /// <summary>
    /// Creates a new instance of <see cref="QueryOptions"/> with the specified format and parallel download option.
    /// </summary>
    /// <param name="format">The format in which the results should be returned.</param>
    /// <param name="downloadInParallel">Indicates whether to download results in parallel.</param>
    /// <returns>A new instance of <see cref="QueryOptions"/>.</returns>
    public static QueryOptions WithFormat(Format format, bool downloadInParallel) => new(format, downloadInParallel);

    /// <summary>
    /// Enables parallel download for the query options.
    /// </summary>
    /// <returns>The current instance of <see cref="QueryOptions"/> with parallel download enabled.</returns>
    public QueryOptions WithParallelDownload()
    {
        DownloadInParallel = true;
        return this;
    }

    /// <summary>
    /// Gets a value indicating whether to download results in parallel.
    /// </summary>
    internal bool DownloadInParallel { get; set; }
}
