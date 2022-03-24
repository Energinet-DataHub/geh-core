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

using System;
using System.Collections.Generic;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware;
using Microsoft.Azure.Functions.Worker.Http;
using Xunit;
using Xunit.Categories;

namespace RequestResponseMiddleware.Tests
{
    [UnitTest]
    public class LogDataBuilderTests
    {
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

        [Fact]
        public void HeaderData_GetDataWithChangedAuthorization()
        {
            // Arrange
            var headerCollection = new HttpHeadersCollection();
            headerCollection.Add("accept", new List<string> { "*/*" });
            headerCollection.Add("acceptencoding", new List<string> { "gzip", "deflate", "br" });
            headerCollection.Add("functionid", new List<string> { "0094d4ad-49af-43f7-b77e-efb732ba0e4f" });
            headerCollection.Add("Authorization", new List<string> { "Bearer ey????iOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6ImpTMVhvMU9XRGpfNTJ2YndHTmd2UU8yVnpNYyJ9.eyJhdWQiOiJjN2U1ZGM1Yy0yZWUwLTQyMGMtYjVkMi01ODZlNzUyNzMwMmMiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vNGE3NDExZWEtYWM3MS00YjYzLTk2NDctYjhiZDRjNWEyMGUwL3YyLjAiLCJpYXQiOjE2NDc5MzQxMDksIm5iZiI6MTY0NzkzNDEwOSwiZXhwIjoxNjQ3OTM4MDA5LCJhaW8iOiJFMlpnWUZnL1RXS0YweGFoajNzZkp2aXZUenhvQXdBPSIsImF6cCI6ImE0YWJkNGFkLTRmZjMtNGU5My1hODZkLTdiMTY0OTEwY2EzNCIsImF6cGFjciI6IjEiLCJvaWQiOiIwNjVjNGM4ZC0zNmE5LTQ3MGQtODliNi1hYjY1M2U1MzU5ZDEiLCJyaCI6IjAuQVNJQTZoRjBTbkdzWTB1V1I3aTlURm9nNEZ6YzVjZmdMZ3hDdGRKWWJuVW5NQ3drQUFBLiIsInJvbGVzIjpbImdyaWRvcGVyYXRvciJdLCJzdWIiOiIwNjVjNGM4ZC0zNmE5LTQ3MGQtODliNi1hYjY1M2U1MzU5ZDEiLCJ0aWQiOiI0YTc0MTFlYS1hYzcxLTRiNjMtOTY0Ny1iOGJkNGM1YTIwZTAiLCJ1dGkiOiJ6dVFqSWNmWnlreTVuTzBUUzVVWkFBIiwidmVyIjoiMi4wIn0.c1CSDatm1E1LOfp5SPMcRyH3DECjdR5t1IUZjLi97ZNzyA-87fZyfTOX67mdPiOZFXMgjPtrZSLZIrZlonGL6y4UtUNPxRXYJL_k6p7e9cvHyhbBB00RZVFlOLUV4YP3eNhJGEhsohAqeT5cZbRNAczmcc4PpqI4Xzc2Qf2FP_NtICqxy0J3-yNig_HLzdtcVUxK70MFSyHYjtZzitNTEC6mLPK4exsEOTzrhzoQLpuiE3XTtQEswaEn_nrh-TuQnZWNLGIQ4z6Abpk6XzHrsmYKi6kHLultEf3W6yg-4GAVT4AMK3o2UGweFSbkTIHNqUf2b13A8kOfjn05xtXrlg" });

            // Act
            var selectedHeaderData = LogDataBuilder.ReadHeaderDataFromCollection(headerCollection);

            // Assert
            Assert.NotEmpty(headerCollection);
            Assert.Contains("Authorization", selectedHeaderData.Keys);
            Assert.Contains("Bearer ****", selectedHeaderData.Values);
        }

        [Fact]
        public void HeaderData_GetDataWithNoAuthorization()
        {
            // Arrange
            var headerCollection = new HttpHeadersCollection();
            headerCollection.Add("accept", new List<string> { "*/*" });
            headerCollection.Add("acceptencoding", new List<string> { "gzip", "deflate", "br" });
            headerCollection.Add("functionid", new List<string> { "0094d4ad-49af-43f7-b77e-efb732ba0e4f" });
            headerCollection.Add("Headers", new List<string> { "{'Accept':'*/*','Accept-Encoding':'gzip,deflate,br','Authorization':'Bearer eyJ0','Content-Length':'2138','X-WAWS-Unencoded-URL':'/api/ChargeIngestion'}" });

            // Act
            var selectedHeaderData = LogDataBuilder.ReadHeaderDataFromCollection(headerCollection);

            // Assert
            Assert.NotEmpty(headerCollection);
            Assert.DoesNotContain("Authorization", selectedHeaderData.Keys);
            Assert.DoesNotContain("Bearer ****", selectedHeaderData.Values);
            Assert.DoesNotContain("Headers", selectedHeaderData.Keys);
        }

        [Fact]
        public void MetaAndIndexTags_BuildMetaAndIndexDataFomContext()
        {
            // Arrange
            var functionContext = new MockedFunctionContext();
            functionContext.FunctionContextMock.Setup(e => e.FunctionId).Returns("123456");
            functionContext.FunctionContextMock.Setup(e => e.InvocationId).Returns(Guid.NewGuid().ToString);
            functionContext.FunctionDefinitionMock.Setup(e => e.Name).Returns("TestName");

            var inputData = new Dictionary<string, object>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "Headers", "{\"Authorization\":\"Bearer ey?????\"}" },
                { "Query", "{ BundleId: 123 }" },
                { "BundleId", "132" },
                { "Correlationid", "132" },
            };

            functionContext.BindingContext
                .Setup(x => x.BindingData)
                .Returns(inputData);

            // Act
            var logData =
                LogDataBuilder.BuildDictionaryFromContext(
                    functionContext.FunctionContext,
                    true);

            // Assert
            Assert.DoesNotContain("query", logData.GetAllTags().Keys);
            Assert.DoesNotContain("headers", logData.GetAllTags().Keys);
            Assert.Contains("functionname", logData.GetAllTags().Keys);
            Assert.Contains("TestName", logData.GetAllTags().Values);
            Assert.Contains("invocationid", logData.GetAllTags().Keys);
            Assert.Contains(logData.GetAllTags(), e => e.Key.Equals("httpdatatype") && e.Value.Equals("request"));
        }
    }
}
