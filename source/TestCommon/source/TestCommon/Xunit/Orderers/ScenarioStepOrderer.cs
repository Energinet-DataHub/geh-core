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

using Energinet.DataHub.Core.TestCommon.Xunit.Attributes;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Energinet.DataHub.Core.TestCommon.Xunit.Orderers;

/// <summary>
/// A custom xUnit test case orderer that executes tests in order according to the attribute <see cref="ScenarioStepAttribute"/>.
/// Use the <see cref="TestCaseOrdererAttribute"/> on a test class to enable the orderer.
///
/// Inspired by: https://learn.microsoft.com/en-us/dotnet/core/testing/order-unit-tests?pivots=xunit#order-by-custom-attribute
/// </summary>
public class ScenarioStepOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        var stepGrouping = GroupByStepNumber(testCases);

        var stepNumbersLowestToHighest = stepGrouping.Keys.Order();

        var sortedTestCases = stepNumbersLowestToHighest
            .SelectMany(stepNumber => stepGrouping[stepNumber]
                .OrderBy(testCase => testCase.TestMethod.Method.Name)); // TODO: Do we want to sort by name?
        foreach (var testcase in sortedTestCases)
        {
            yield return testcase;
        }
    }

    private SortedDictionary<int, List<TTestCase>> GroupByStepNumber<TTestCase>(
        IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        var assemblyName = typeof(ScenarioStepAttribute).AssemblyQualifiedName!;

        var sortedMethods = new SortedDictionary<int, List<TTestCase>>();
        foreach (var testCase in testCases)
        {
            var stepNumber = testCase.TestMethod.Method
                .GetCustomAttributes(assemblyName)
                .FirstOrDefault()
                ?.GetNamedArgument<int>(nameof(ScenarioStepAttribute.Number)) ?? 0;

            GetOrCreate(sortedMethods, stepNumber).Add(testCase);
        }

        return sortedMethods;
    }

    private static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
        where TKey : struct
        where TValue : new()
    {
        return dictionary.TryGetValue(key, out var result)
            ? result
            : dictionary[key] = new TValue();
    }
}
