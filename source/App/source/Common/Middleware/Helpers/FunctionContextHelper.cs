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
using System.Net;
using Energinet.DataHub.Core.App.Common.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.Core.App.Common.Middleware.Helpers
{
    public static class FunctionContextHelper
    {
        internal static void SetErrorResponse(FunctionContext context)
        {
            var httpRequestData = context.GetHttpRequestData() ?? throw new InvalidOperationException();
            var httpResponseData = httpRequestData.CreateResponse(HttpStatusCode.Unauthorized);

            context.SetHttpResponseData(httpResponseData);
        }
    }
}
