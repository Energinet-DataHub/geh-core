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
using Energinet.DataHub.Core.App.Common.Users;
using ExampleHost.FunctionApp01.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ExampleHost.FunctionApp01.Functions;

public class AuthUserId
{
    private readonly IUserContext<ExampleSubsystemUser> _currentUser;

    public AuthUserId(UserContext<ExampleSubsystemUser> currentUser)
     {
         _currentUser = currentUser;
     }

    [Function(nameof(Auth))]
    public string Auth(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "authentication/user")]
        HttpRequestData httpRequest)
    {
        return _currentUser.CurrentUser.UserId.ToString();
    }
}
