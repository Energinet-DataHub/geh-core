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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Extensions;
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
                await LogResponseAsync(responseLogInformation, requestLogInformation.MetaData).ConfigureAwait(false);

                totalTimer.Stop();
                _logger.LogInformation("RequestResponse Total execution time ms: {}", totalTimer.ElapsedMilliseconds);
            }
            else
            {
                await next(context).ConfigureAwait(false);
            }
        }

        private Task LogRequestAsync(LogInformation requestLogInformation)
        {
            var requestLogNameAndFolder = LogDataBuilder.BuildLogName(requestLogInformation.MetaData);
            var requestLogName = requestLogNameAndFolder.Name + "_request.txt";
            return _requestResponseLogging.LogRequestAsync(requestLogInformation.LogStream, requestLogInformation.MetaData, requestLogInformation.IndexTags, requestLogName, requestLogNameAndFolder.Folder);
        }

        private Task LogResponseAsync(LogInformation responseLogInformation, Dictionary<string, string> requestMetaData)
        {
            var responseLogNameAndFolder = LogDataBuilder.BuildLogName(requestMetaData);
            var responseLogName = responseLogNameAndFolder.Name + "_response.txt";
            return _requestResponseLogging.LogResponseAsync(responseLogInformation.LogStream, responseLogInformation.MetaData, responseLogInformation.IndexTags, responseLogName, responseLogNameAndFolder.Folder);
        }

        private static async Task<LogInformation> BuildRequestLogInformationAsync(FunctionContext context)
        {
            var (metaData, indexTags) = LogDataBuilder.GetMetaDataAndIndexTagsDictionaries(context, true);

            if (context.GetHttpRequestData() is { } requestData)
            {
                foreach (var (key, value) in LogDataBuilder.ReadHeaderDataFromCollection(requestData.Headers))
                {
                    metaData.TryAdd(LogDataBuilder.MetaNameFormatter(key), value);
                }

                var streamToLog = new MemoryStream();
                await requestData.Body.CopyToAsync(streamToLog);
                requestData.Body.Position = 0;
                streamToLog.Position = 0;

                return new LogInformation(streamToLog, metaData, indexTags);
            }

            return new LogInformation(Stream.Null, metaData, indexTags);
        }

        private static async Task<LogInformation> BuildResponseLogInformationAsync(FunctionContext context)
        {
            var (metaData, indexTags) = LogDataBuilder.GetMetaDataAndIndexTagsDictionaries(context, false);

            if (context.GetHttpResponseData() is { } responseData)
            {
                foreach (var (key, value) in LogDataBuilder.ReadHeaderDataFromCollection(responseData.Headers))
                {
                    metaData.TryAdd(LogDataBuilder.MetaNameFormatter(key), value);
                }

                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("StatusCode"), responseData.StatusCode.ToString());
                indexTags.TryAdd(LogDataBuilder.MetaNameFormatter("StatusCode"), responseData.StatusCode.ToString());

                await PrepareResponseStreamForLoggingAsync(responseData);

                var streamToLog = await ResponseStreamReader.CopyBodyStreamAsync(responseData.Body);

                await PrepareResponseStreamToReturnAsync(responseData, streamToLog);

                return new LogInformation(streamToLog, metaData, indexTags);
            }

            return new LogInformation(Stream.Null, metaData, indexTags);
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
                return request is { };
            }
            catch
            {
                return false;
            }
        }
    }
}
