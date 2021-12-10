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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.Core.Logging.RequestResponseMiddleware
{
    public class RequestResponseLoggingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IRequestResponseLogging _requestResponseLogging;

        public RequestResponseLoggingMiddleware(IRequestResponseLogging requestResponseLogging)
        {
            _requestResponseLogging = requestResponseLogging;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var requestLog = BuildRequestLogInformation(context);
            await _requestResponseLogging.LogRequestAsync(requestLog.LogStream, requestLog.MetaData).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);

            var responseLog = BuildResponse(context);
            await _requestResponseLogging.LogResponseAsync(responseLog.LogStream, responseLog.MetaData).ConfigureAwait(false);
        }

        private static (Stream LogStream, Dictionary<string, string> MetaData) BuildRequestLogInformation(FunctionContext context)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(context.BindingContext.BindingData.ToString() ?? string.Empty);
            var dictionary = context.BindingContext.BindingData.ToDictionary(e => e.Key, pair => pair.Value as string);
            return new ValueTuple<Stream, Dictionary<string, string>>(new MemoryStream(bytes), dictionary);
        }

        private static (Stream LogStream, Dictionary<string, string> MetaData) BuildResponse(FunctionContext context)
        {
            var functionBindingsFeature = context.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature").Value;
            if (functionBindingsFeature == null)
            {
                throw new ArgumentException("Cannot get function bindings feature, IFunctionBindingsFeature");
            }

            var type = functionBindingsFeature.GetType();
            var result = type.GetProperties().Single(p => p.Name == "InvocationResult");

            if (result.GetValue(functionBindingsFeature) is HttpResponseData responseData)
            {
                var metaData = ReadHeaderData(responseData.Headers);
                metaData.Add("StatusCode", responseData.StatusCode.ToString());

                return new ValueTuple<Stream, Dictionary<string, string>>(responseData.Body, metaData);
            }

            return new ValueTuple<Stream, Dictionary<string, string>>(Stream.Null, new Dictionary<string, string>());
        }

        private static Dictionary<string, string> ReadHeaderData(HttpHeadersCollection headersCollection)
        {
            var metaData = new Dictionary<string, string>();
            foreach (var (key, value) in headersCollection)
            {
                metaData.Add(key, value.ToString() ?? string.Empty);
            }

            return metaData;
        }
    }
}
