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
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Models;
using Xunit;
using Xunit.Categories;

namespace RequestResponseMiddleware.Tests
{
    [UnitTest]
    public class LogTagsTests
    {
        [Fact]
        public void LogTags_AllIndexTagsExists()
        {
            // Arrange
            var logTags = new LogTags();

            var queryTags = new Dictionary<string, string>()
            {
                { "bundleId", "944aaf2e-aa6b-4789-b34c-d19aca4bdef8" },
                { "marketOperator", "name" },
                { "userId", "1234" },
                { "userId1", "1234" },
                { "userId2", "1234" },
                { "userId3", "1234" },
                { "userId4", "1234" },
                { "userId5", "1234" },
            };
            logTags.AddQueryTagsCollection(queryTags);

            var jwtActorId = Guid.NewGuid().ToString();
            var functionName = "TestFunctionName";
            var functionId = Guid.NewGuid().ToString();
            var invocationId = Guid.NewGuid().ToString();
            var traceParent = "00-d3ff2b9ea8e4b3488ef1b0cd785851b5-4aeeadc281bd0940-00";
            var traceId = "d3ff2b9ea8e4b3488ef1b0cd785851b5";

            var contextTags = new Dictionary<string, string>()
            {
                { IndexTagsKeys.JwtActorId, jwtActorId },
                { IndexTagsKeys.FunctionId, functionId },
                { IndexTagsKeys.FunctionName, functionName },
                { IndexTagsKeys.InvocationId, invocationId },
                { IndexTagsKeys.TraceParent, traceParent },
                { IndexTagsKeys.TraceId, traceId },
                { "UnknownTag1", "item1" },
                { "UnknownTag2", "item2" },
                { "UnknownTag3", "item3" },
                { "UnknownTag4", "item4" },
                { "UnknownTag5", "item5" },
                { "UnknownTag6", "item6" },
            };
            logTags.AddContextTagsCollection(contextTags);

            var correlationId = Guid.NewGuid().ToString();
            var headerTags = new Dictionary<string, string>()
            {
                { IndexTagsKeys.CorrelationId, correlationId },
                { "acceptencoding", "gzip,deflate,br" },
                { "contenttype", "application/xml" },
            };
            logTags.AddHeaderCollectionTags(headerTags);

            // Act
            var allTags = logTags.GetAllTags();
            var metaDataJsonString = logTags.BuildMetaDataForLog();
            var allIndexTags = logTags.GetAllIndexTags();
            var max10IndexTags = logTags.GetIndexTagsWithMax10Items();

            // Assert
            var totalTags = queryTags.Count + contextTags.Count + headerTags.Count;
            Assert.Equal(10, max10IndexTags.Count);
            Assert.Equal(totalTags, allTags.Count);
            Assert.Equal(7, allIndexTags.Count);
            Assert.True(metaDataJsonString["indextags"].Length > 50);
            Assert.True(metaDataJsonString.Count > allTags.Count);
        }
    }
}
