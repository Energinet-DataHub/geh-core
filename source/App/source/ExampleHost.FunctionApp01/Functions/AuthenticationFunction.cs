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

using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using ExampleHost.FunctionApp01.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleHost.FunctionApp01.Functions;

public class AuthenticationFunction
{
    /// <summary>
    /// This method should be called with a 'Bearer' token in the 'Authorization' header.
    /// The token must be a nested token (containing both external and internal token).
    /// From this token the method must retrieve the UserId and return it, for tests to verify.
    /// If the user is not available an Empty guid is returned.
    /// </summary>
    [Function(nameof(GetUserWithPermission))]
    public string GetUserWithPermission(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "authentication/user")]
        HttpRequestData httpRequest,
        FunctionContext context)
    {
        try
        {
            // TODO: Add 'FunctionContext' extension to allow easy access to "CurrentUser"
            var userContext = context.InstanceServices.GetRequiredService<IUserContext<ExampleSubsystemUser>>();
            return userContext.CurrentUser.UserId.ToString();
        }
        catch
        {
            return Guid.Empty.ToString();
        }
    }
}
