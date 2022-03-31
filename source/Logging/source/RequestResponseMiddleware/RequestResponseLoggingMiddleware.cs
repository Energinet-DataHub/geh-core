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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Extensions;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Models;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Storage;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Core.Logging.RequestResponseMiddleware
{
    public class RequestResponseLoggingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IRequestResponseLogging _requestResponseLogging;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(
            IRequestResponseLogging requestResponseLogging,
            ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _requestResponseLogging = requestResponseLogging;
            _logger = logger;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var shouldLogRequestAndResponse = ShouldLogRequestResponse(context);

            if (shouldLogRequestAndResponse)
            {
                _logger.LogInformation($"RequestResponse: Starting logging for invocation: {context.InvocationId}");
                var totalTimer = Stopwatch.StartNew();

                // Starts gathering information from request and logs to storage
                var requestLogInformation = await BuildRequestLogInformationAsync(context);
                await LogRequestAsync(requestLogInformation).ConfigureAwait(false);

                totalTimer.Stop();

                // Calls next middleware
                await next(context).ConfigureAwait(false);

                totalTimer.Start();

                // Starts gathering information from response and logs to storage
                var responseLogInformation = await BuildResponseLogInformationAsync(context);
                await LogResponseAsync(responseLogInformation).ConfigureAwait(false);

                totalTimer.Stop();
                _logger.LogInformation("RequestResponse: Total execution time ms: {}", totalTimer.ElapsedMilliseconds);
            }
            else
            {
                await next(context).ConfigureAwait(false);
            }
        }

        private Task LogRequestAsync(LogInformation requestLogInformation)
        {
            var requestLogName = LogDataBuilder.BuildLogName();
            return _requestResponseLogging.LogRequestAsync(requestLogInformation.LogStream, requestLogInformation.MetaData, requestLogInformation.IndexTags, requestLogName);
        }

        private Task LogResponseAsync(LogInformation responseLogInformation)
        {
            var responseLogName = LogDataBuilder.BuildLogName();
            return _requestResponseLogging.LogResponseAsync(responseLogInformation.LogStream, responseLogInformation.MetaData, responseLogInformation.IndexTags, responseLogName);
        }

        private static async Task<LogInformation> BuildRequestLogInformationAsync(FunctionContext context)
        {
            var uniqueLogName = LogDataBuilder.BuildLogName();
            var logTags = LogDataBuilder.BuildDictionaryFromContext(context, true);
            logTags.AddToHeaderCollection(IndexTagsKeys.UniqueLogName, uniqueLogName);

            if (context.GetHttpRequestData() is { } requestData)
            {
                logTags.AddHeaderCollectionTags(LogDataBuilder.ReadHeaderDataFromCollection(requestData.Headers));

                var streamToLog = new MemoryStream();
                await requestData.Body.CopyToAsync(streamToLog);
                requestData.Body.Position = 0;
                streamToLog.Position = 0;

                return new LogInformation(streamToLog, logTags.BuildMetaDataForLog(), logTags.GetIndexTagsWithMax10Items(), uniqueLogName);
            }

            return new LogInformation(Stream.Null, logTags.BuildMetaDataForLog(), logTags.GetIndexTagsWithMax10Items(), uniqueLogName);
        }

        private static async Task<LogInformation> BuildResponseLogInformationAsync(FunctionContext context)
        {
            var uniqueLogName = LogDataBuilder.BuildLogName();
            var logTags = LogDataBuilder.BuildDictionaryFromContext(context, false);
            logTags.AddToHeaderCollection(IndexTagsKeys.UniqueLogName, uniqueLogName);

            if (context.GetHttpResponseData() is { } responseData)
            {
                logTags.AddToHeaderCollection(LogDataBuilder.MetaNameFormatter("StatusCode"), responseData.StatusCode.ToString());
                logTags.AddHeaderCollectionTags(LogDataBuilder.ReadHeaderDataFromCollection(responseData.Headers));

                await PrepareResponseStreamForLoggingAsync(responseData);

                var streamToLog = await ResponseStreamReader.CopyBodyStreamAsync(responseData.Body);

                await PrepareResponseStreamToReturnAsync(responseData, streamToLog);

                return new LogInformation(streamToLog, logTags.BuildMetaDataForLog(), logTags.GetIndexTagsWithMax10Items(), uniqueLogName);
            }

            return new LogInformation(Stream.Null, logTags.BuildMetaDataForLog(), logTags.GetIndexTagsWithMax10Items(), uniqueLogName);
        }

        private static async Task PrepareResponseStreamToReturnAsync(HttpResponseData responseData, Stream streamToLog)
        {
            if (responseData.Body.CanSeek)
            {
                responseData.Body.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                await responseData.Body.DisposeAsync();
                var responseStream = await ResponseStreamReader.CopyBodyStreamAsync(streamToLog);
                streamToLog.Seek(0, SeekOrigin.Begin);
                responseData.Body = responseStream;
            }
        }

        private static async Task PrepareResponseStreamForLoggingAsync(HttpResponseData responseData)
        {
            if (responseData.Body.Position > 0)
            {
                if (responseData.Body.CanSeek)
                {
                    responseData.Body.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    await responseData.Body.DisposeAsync();
                    throw new InvalidOperationException("Can not log response stream because it is not seekable");
                }
            }
        }

        private bool ShouldLogRequestResponse(FunctionContext context)
        {
            try
            {
                var request = context.GetHttpRequestData();
                return request is { } req && !req.Url.AbsoluteUri.Contains("/monitor/", StringComparison.InvariantCultureIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
