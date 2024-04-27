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

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ExampleHost.FunctionApp01.Functions;

public class AuthenticationFunction
{
    public AuthenticationFunction()
    {
    }

    /// <summary>
    /// This method should be called with a 'Bearer' token in the 'Authorization' header.
    /// The token must be a nested token (containing both external and internal token).
    /// From this token the method must retrieve the UserId and return it, for tests to verify.
    /// </summary>
    [Function(nameof(GetUserWithPermission))]
    public string GetUserWithPermission(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "authentication/user")]
        HttpRequestData httpRequest)
    {
        // TODO: Retrieve UserId from token
        return Guid.NewGuid().ToString();
    }
}
