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

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;

internal static class ReflectionsLambda
{
    private static readonly ConcurrentDictionary<Type, Delegate> ConstructorCache = new();

    public static string[] GetArrowFieldNames<T>()
        => ArrowFieldNamesFromProperties<T>._names;

    /// <summary>
    /// Creates an instance of <typeparamref name="T" /> using the constructor
    /// </summary>
    /// <param name="args">Arguments for the constructor</param>
    /// <typeparam name="T">Object type to create</typeparam>
    /// <returns>Hydrated object</returns>
    /// <exception cref="InvalidOperationException">If there is less or more then one constructor</exception>
    /// <exception cref="ArgumentException">The supplied values does not match</exception>
    public static T CreateInstance<T>(params object?[] args)
    {
        var type = typeof(T);
        if (!ConstructorCache.TryGetValue(type, out var cachedDelegate))
        {
            var constructor = type.GetConstructors().Single();
            var ctorParams = constructor.GetParameters();
            var argsParam = Expression.Parameter(typeof(object[]), "args");
            var parameters = ctorParams
                .Select((p, i) => Expression.Convert(Expression.ArrayIndex(argsParam, Expression.Constant(i)), p.ParameterType))
                .ToArray();

            var constructorCall = Expression.New(constructor, parameters);

            var argCountCheck = Expression.IfThen(
                Expression.NotEqual(Expression.ArrayLength(argsParam), Expression.Constant(ctorParams.Length)),
                Expression.Throw(Expression.New(typeof(ArgumentException).GetConstructor(new[] { typeof(string) })!, Expression.Constant($"Expected {ctorParams.Length} arguments but got {args.Length}"))));

            var block = Expression.Block(argCountCheck, constructorCall);
            var lambda = Expression.Lambda<Func<object?[], T>>(block, argsParam).Compile();
            ConstructorCache[type] = lambda;
            cachedDelegate = lambda;
        }

        if (cachedDelegate == null)
        {
            throw new InvalidOperationException($"No constructor found for type {type.Name}");
        }

        var obj = ((Func<object?[], T>)cachedDelegate)(args) ?? throw new InvalidOperationException($"Failed to create instance of type {type.Name}");

        return (T)obj;
    }

    private static class ArrowFieldNamesFromProperties<T>
    {
        public static readonly string[] _names = GetNamesByConstructorOrder();

        private static string[] GetNamesByConstructorOrder()
        {
            // Restrict the search to public instance properties
            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // Filter properties with the ArrowFieldAttribute, order and select names in one query
            return properties
                .SelectMany(
                    p => p.GetCustomAttributes(typeof(ArrowFieldAttribute), false)
                    .OfType<ArrowFieldAttribute>()
                    .Select(attr => new { attr.ConstructorOrder, attr.Name }))
                .OrderBy(attr => attr.ConstructorOrder)
                .Select(attr => attr.Name)
                .ToArray();
        }
    }
}
