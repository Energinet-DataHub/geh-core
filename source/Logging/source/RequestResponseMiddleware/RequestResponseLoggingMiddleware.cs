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

            var responseLog = BuildResponseLogInformation(context);
            await _requestResponseLogging.LogResponseAsync(responseLog.LogStream, responseLog.MetaData).ConfigureAwait(false);
        }

        private static (Stream LogStream, Dictionary<string, string> MetaData) BuildRequestLogInformation(FunctionContext context)
        {
            var bindingsFeature = context.GetHttpRequestData();

            var metaData = context.BindingContext.BindingData.ToDictionary(e => e.Key, pair => pair.Value as string);
            if (bindingsFeature is { } requestData)
            {
                foreach (var (key, value) in ReadHeaderDataFromCollection(requestData.Headers))
                {
                    metaData.TryAdd(key, value);
                }

                metaData.TryAdd("FunctionId", context.FunctionId);
                metaData.TryAdd("InvocationId", context.InvocationId);
                metaData.TryAdd("TraceParent", context.TraceContext?.TraceParent ?? string.Empty);

                // TODO Should we "reset" stream ?
                return new ValueTuple<Stream, Dictionary<string, string>>(requestData.Body, metaData);
            }

            return new ValueTuple<Stream, Dictionary<string, string>>(Stream.Null, metaData);
        }

        private static (Stream LogStream, Dictionary<string, string> MetaData) BuildResponseLogInformation(FunctionContext context)
        {
            var metaData = context.BindingContext.BindingData.ToDictionary(e => e.Key, pair => pair.Value as string ?? string.Empty);
            if (context.GetHttpResponseData() is { } responseData)
            {
                foreach (var (key, value) in ReadHeaderDataFromCollection(responseData.Headers))
                {
                    metaData.TryAdd(key, value);
                }

                metaData.TryAdd("StatusCode", responseData.StatusCode.ToString());
                metaData.TryAdd("FunctionId", context.FunctionId);
                metaData.TryAdd("InvocationId", context.InvocationId);
                metaData.TryAdd("TraceParent", context.TraceContext?.TraceParent ?? string.Empty);

                // TODO Should we "reset" stream ?
                return new ValueTuple<Stream, Dictionary<string, string>>(responseData.Body, metaData);
            }

            return new ValueTuple<Stream, Dictionary<string, string>>(Stream.Null, metaData);
        }

        private static Dictionary<string, string> ReadHeaderDataFromCollection(HttpHeadersCollection headersCollection)
        {
            if (headersCollection is null)
            {
                return new Dictionary<string, string>();
            }

            var metaData = headersCollection
                .ToDictionary(e => e.Key, e => string.Join(",", e.Value));

            return metaData;
        }
    }
}
