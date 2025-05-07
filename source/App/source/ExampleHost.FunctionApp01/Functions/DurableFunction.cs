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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;

namespace ExampleHost.FunctionApp01.Functions;

/// <summary>
/// Demonstrate our middleware configuration work in combination with the execution of
/// Durable Function orchestration and activity triggers.
/// </summary>
/// <remarks>
/// The implementation of this class is heavily inspired by the quickstart for Durable Functions app.
/// IMPORTANT: Never use ConfigureAwait in orchestration code.
/// </remarks>
public class DurableFunction
{
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
    /// <remarks>
    /// IMPORTANT: Never use ConfigureAwait in orchestration code.
    /// </remarks>
    [Function(nameof(RunOrchestrator))]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger]
        TaskOrchestrationContext context)
    {
        var outputs = new List<string>();

        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

        // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
        return outputs;
    }
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

    [Function(nameof(SayHello))]
    public static string SayHello(
        [ActivityTrigger]
        string name)
    {
        return $"Hello {name}!";
    }

    /// <summary>
    /// Used in tests to trigger an Durable Function orchestration, that again triggers
    /// the execution of multiple activities.
    /// The purpose is to verify that our middleware doesn't hinder the orchestration to
    /// be completed.
    /// Read this issues to understand the reasoning behind this test: https://github.com/Azure/azure-functions-dotnet-worker/issues/1666
    /// </summary>
    [Function(nameof(ExecuteDurableFunction))]
    public static async Task<IActionResult> ExecuteDurableFunction(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "durable")]
        HttpRequest request,
        [DurableClient]
        DurableTaskClient client)
    {
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(RunOrchestrator)).ConfigureAwait(false);

        var metadata = await client.WaitForInstanceCompletionAsync(instanceId).ConfigureAwait(false);
        return new OkObjectResult(metadata);
    }
}
