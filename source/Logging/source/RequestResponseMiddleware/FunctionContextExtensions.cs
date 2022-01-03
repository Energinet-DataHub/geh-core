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
using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.Core.Logging.RequestResponseMiddleware
{
    internal static class FunctionContextExtensions
    {
        public static HttpRequestData? GetHttpRequestData(this FunctionContext functionContext)
        {
            try
            {
                var functionBindingsFeature = functionContext?.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature").Value;
                if (functionBindingsFeature is null)
                {
                    throw new ArgumentException("Cannot get function bindings feature, IFunctionBindingsFeature");
                }

                var type = functionBindingsFeature.GetType();
                var inputData = type.GetProperties().Single(p => p.Name == "InputData").GetValue(functionBindingsFeature) as IReadOnlyDictionary<string, object>;

                var requestData = inputData?.Values.SingleOrDefault(o => o is HttpRequestData) as HttpRequestData;

                return (requestData ?? null) ?? throw new InvalidOperationException();
            }
            catch
            {
                return null;
            }
        }

        public static HttpResponseData? GetHttpResponseData(this FunctionContext functionContext)
        {
            try
            {
                var functionBindingsFeature = functionContext.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature").Value;
                if (functionBindingsFeature is null)
                {
                    throw new ArgumentException("Cannot get function bindings feature, IFunctionBindingsFeature");
                }

                var type = functionBindingsFeature.GetType();
                var invocationResult = type.GetProperties().Single(p => p.Name == "InvocationResult");

                if (invocationResult.GetValue(functionBindingsFeature) is HttpResponseData responseData)
                {
                    return responseData;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
