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

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware;
using Xunit;
using Xunit.Categories;

namespace RequestResponseMiddleware.Tests
{
    [UnitTest]
    public class LogDataBuilderTests
    {
        private static readonly JwtSecurityTokenHandler _tokenHandler = new();

        [Theory]
        [InlineData("00-11f7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01", true)]
        [InlineData("00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01", true)]
        [InlineData("0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01", false)]
        [InlineData("b7ad6b7169203331-01", false)]
        [InlineData("01", false)]
        [InlineData("-01", false)]
        public void TraceParentSplit_ValidTraceParent(string traceParent, bool parseOk)
        {
            var result = LogDataBuilder.TraceParentSplit(traceParent);
            Assert.True(result is { } == parseOk);
        }

        [Theory]
        [InlineData("00-11f7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01", "11f7651916cd43dd8448eb211c80319c")]
        [InlineData("00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01", "0af7651916cd43dd8448eb211c80319c")]
        public void TraceParentSplit_ValidTraceId(string traceParent, string expectedTraceId)
        {
            var result = LogDataBuilder.TraceParentSplit(traceParent);
            Assert.NotNull(result);
            Assert.Equal(expectedTraceId, result.Value.Traceid);
        }

        [Theory]
        [InlineData("11f7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01")]
        [InlineData(null)]
        [InlineData("")]
        public void TraceParentSplit_InValidTraceId(string? traceParent)
        {
            var result = LogDataBuilder.TraceParentSplit(traceParent ?? string.Empty);
            Assert.Null(result);
        }
    }
}
