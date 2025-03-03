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

using System.Diagnostics;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;

public static class DebugInfo
{
    private static readonly Dictionary<string, int> _counters = new();
    private static readonly Dictionary<string, TimeSpan> _timeSpans = new();

    public static void IncrementCounter(string counterName)
    {
        _counters.TryAdd(counterName, 0);
        _counters[counterName]++;
    }

    public static void ResetCounters() => _counters.Clear();

    public static void ResetMeasurements() => _timeSpans.Clear();

    public static int GetCounter(string counterName) => _counters.GetValueOrDefault(counterName, 0);

    public static (string CounterName, int Count)[] GetCounters() => _counters.Select(kvp => (kvp.Key, kvp.Value)).ToArray();

    public static void PrintCounters(Action<string, object?[]> printer)
    {
        foreach (var (counterName, count) in GetCounters()) printer("Counter: {0} - Count: {1}", [counterName, count]);
    }

    public static void PrintMeasurements(Action<string, object?[]> printer)
    {
        foreach (var (counterName, timeSpan) in _timeSpans) printer("Counter: {0} - Accumulated time: {1}", [counterName, timeSpan]);
    }

    public static T Measure<T>(string counterName, Func<T> action)
    {
        var stopwatch = Stopwatch.StartNew();
        var actionOutput = action();
        stopwatch.Stop();

        _timeSpans.TryAdd(counterName, TimeSpan.Zero);
        _timeSpans[counterName] += stopwatch.Elapsed;

        return actionOutput;
    }
}
