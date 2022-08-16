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

using Energinet.DataHub.Core.App.FunctionApp.FunctionTelemetryScope;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.App.FunctionApp.Tests.Middleware.CorrelationId
{
    public class TraceParentTests
    {
        [Theory]
        [InlineData("", false)]
        [InlineData("00,0af7651916cd43dd8448eb211c80319c,b9c7c989f97918e1,00", false)] // wrong separator used
        [InlineData("00-0af7651916cd43dd8448eb211c80319-b9c7c989f97918e1-00-", false)] // TraceParent > 55
        [InlineData("00-0af7651916cd43dd8448eb211c80319-b9c7c989f97918e1-0", false)] // TraceParent < 55
        [InlineData("00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1", false)] // parts < 4
        [InlineData("00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-00-1", false)] // parts > 4
        [InlineData("00-0af7651916cd43dd8448eb211c80319-b9c7c989f97918e1-00", false)] // TraceId < 32
        [InlineData("00-0af7651916cd43dd8448eb211c80319cd-b9c7c989f97918e1-00", false)] // TraceId > 32
        [InlineData("00-0af7651916cd43dd8448eb211c80319-b9c7c989f97918e-00", false)] // ParentId < 16
        [InlineData("00-0af7651916cd43dd8448eb211c80319-b9c7c989f97918e12-00", false)] // ParentId > 16
        [InlineData("01-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-00", false)] // Version != 01
        [InlineData("00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-00", true)]
        public void TraceParentShouldParse(string traceParentString, bool validated)
        {
            var traceParent = TraceParent.Parse(traceParentString);

            traceParent.IsValid.Should().Be(validated);
        }
    }
}
