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

using AutoFixture;

namespace Energinet.DataHub.Core.TestCommon.AutoFixture.Extensions;

public static class AutoFixtureExtensions
{
    /// <summary>
    /// Fluent API for configuring object builder to use a given value for a constructor parameter.
    ///
    /// Inspired by: https://stackoverflow.com/questions/16819470/autofixture-automoq-supply-a-known-value-for-one-constructor-parameter/16954699#16954699
    /// </summary>
    /// <example>
    /// <code>
    /// Notice generic brackets cannot be used in xml documentation example, so we use {}.
    /// var sut = new Fixture()
    ///     .ForConstructorOn{TClass}()
    ///     .SetParameter("value1").To(aValue)
    ///     .SetParameter("value2").ToEnumerableOf(22, 33)
    ///     .Create();
    /// </code>
    /// </example>
    public static SetParameterCreateProvider<TTypeToConstruct> ForConstructorOn<TTypeToConstruct>(this IFixture fixture)
    {
        return new SetParameterCreateProvider<TTypeToConstruct>(fixture);
    }
}
