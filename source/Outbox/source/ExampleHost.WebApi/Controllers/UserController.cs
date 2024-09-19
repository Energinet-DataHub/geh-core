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

using Energinet.DataHub.Core.Outbox.Abstractions;
using ExampleHost.WebApi.DbContext;
using ExampleHost.WebApi.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace ExampleHost.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(CreateUserService createUserService) : ControllerBase
{
    private readonly CreateUserService _createUserService = createUserService;

    /// <summary>
    /// Perform the following steps as an example of using the outbox:
    ///
    /// 1) Perform some business logic (ie. create a user in a <see cref="CreateUserService"/>)
    ///
    /// 2) Create an outbox message using an <see cref="IOutboxClient"/> (in the same transaction)
    ///
    /// 3) Save changes on the <see cref="MyApplicationDbContext"/>, to ensure a user is created and an outbox
    /// message is created in the same transaction
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser(string email)
    {
        // The CreateUserService service creates a user in the database and sends en email (through the outbox)
        await _createUserService.CreateAsync(email)
            .ConfigureAwait(false);

        return new OkResult();
    }
}
