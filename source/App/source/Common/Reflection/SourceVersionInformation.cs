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

namespace Energinet.DataHub.Core.App.Common.Reflection;

/// <summary>
/// Our workflow in GitHub adds DH3 source version information to the <see cref="System.Reflection.AssemblyInformationalVersionAttribute"/>
/// during build. Using this we can identify the exact source code version that was used to build the assembly.
/// This class is used to carry the parsed information.
/// See also <seealso cref="AssemblyInformationalVersionAttributeExtensions.GetSourceVersionInformation(System.Reflection.AssemblyInformationalVersionAttribute)"/>.
/// </summary>
public class SourceVersionInformation
{
    /// <summary>
    /// Used for construction if we don't have the full DH3 source version information.
    /// </summary>
    public SourceVersionInformation(string productVersion)
    {
        ProductVersion = productVersion ?? throw new ArgumentNullException(nameof(productVersion));
        PullRequestNumber = string.Empty;
        LastMergeCommitSha = string.Empty;
    }

    /// <summary>
    /// Used for construction if we do have the full DH3 source version information.
    /// </summary>
    public SourceVersionInformation(string productVersion, string pullRequestNumber, string lastMergeCommitSha)
    {
        ProductVersion = productVersion ?? throw new ArgumentNullException(nameof(productVersion));
        PullRequestNumber = pullRequestNumber ?? throw new ArgumentNullException(nameof(pullRequestNumber));
        LastMergeCommitSha = lastMergeCommitSha ?? throw new ArgumentNullException(nameof(lastMergeCommitSha));
    }

    public string ProductVersion { get; }

    public string PullRequestNumber { get; }

    /// <summary>
    /// Last merge commit on the 'GITHUB_REF' branch (the PR branch, not the feature branch).
    /// See: https://docs.github.com/en/actions/using-workflows/events-that-trigger-workflows#pull_request
    /// </summary>
    public string LastMergeCommitSha { get; }

    public override string ToString()
    {
        return string.IsNullOrEmpty(PullRequestNumber) || string.IsNullOrEmpty(LastMergeCommitSha)
            ? ProductVersion
            : $"Version: {ProductVersion} PR: {PullRequestNumber} SHA: {LastMergeCommitSha}";
    }
}
