﻿// Copyright 2020 Energinet DataHub A/S
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

namespace ExampleHost.FunctionApp01.Common;

/// <summary>
/// Contains names of settings used by functions.
/// </summary>
public static class EnvironmentSettingNames
{
    public const string IntegrationEventFullyQualifiedNamespace = "INTEGRATIONEVENT_FULLY_QUALIFIED_NAMESPACE";
    public const string IntegrationEventTopicName = "INTEGRATIONEVENT_TOPIC_NAME";
}
