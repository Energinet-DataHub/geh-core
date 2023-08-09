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

using Energinet.DataHub.Core.App.WebApp.Hosting;

namespace ExampleHost.WebApi01;

public class SomeTrigger : RepeatingTrigger<SomeTrigger.SomeWorker>
{
    public SomeTrigger(IServiceProvider serviceProvider, ILogger<SomeTrigger> logger)
        : base(serviceProvider, logger, TimeSpan.FromMilliseconds(100))
    {
    }

    public class SomeWorker
    {
        private readonly Thrower _thrower;

        public SomeWorker(Thrower thrower)
        {
            _thrower = thrower;
        }

        public void DoWork(ILogger logger)
        {
            logger.LogInformation("{ServiceName} invoked", nameof(SomeTrigger));
            _thrower.Execute();
        }

        /// <summary>
        /// As the worker service should be scoped we add a singleton thrower to be able to control the health of the worker from tests.
        /// </summary>
        public class Thrower
        {
            public bool IsThrowing { get; set; }

            public void Execute()
            {
                if (IsThrowing)
                {
                    throw new Exception("Not healthy");
                }
            }
        }
    }

    protected override Task ExecuteAsync(SomeWorker scopedService, CancellationToken cancellationToken, Action isAliveCallback)
    {
        scopedService.DoWork(Logger);
        return Task.CompletedTask;
    }
}
