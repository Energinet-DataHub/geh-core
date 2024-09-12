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
using NodaTime;

namespace ExampleHost.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OutboxProcessorController(IOutboxProcessor outboxProcessor) : ControllerBase
{
    private readonly IOutboxProcessor _outboxProcessor = outboxProcessor;

    /// <summary>
    /// Use the <see cref="IOutboxProcessor"/> to process the outbox. This is in a http controller to make it
    /// easy to trigger in tests, but in a real world scenario this should be triggered on a schedule by
    /// a background service or a timer trigger.
    /// </summary>
    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        // Process all waiting outbox messages in the outbox
        await _outboxProcessor.ProcessOutboxAsync(
                limit: 1000,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new OkResult();
    }
}
