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

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;

internal static class ReflectionsCache
{
    private static readonly ConcurrentDictionary<Type, string[]> ArrowFieldNamesCache = new();
    private static readonly ConcurrentDictionary<Type, Func<object?[], object>> ConstructorCache = new();
    private static readonly ConcurrentDictionary<Type, Func<object?[], bool>> ValidateValuesCache = new();

    public static string[] GetArrowFieldNames<T>()
        => ArrowFieldNamesCache.GetOrAdd(typeof(T), _ => ArrowFieldNamesFromProperties<T>._names);

    public static T CreateInstance<T>(params object?[] values)
    {
        var type = typeof(T);
        var isConstructorValid = Activator<T>.ValidateConstructor();
        var valuesMatchConstructor = ValidateValuesCache.GetOrAdd(type, _ => Activator<T>.ValidateValues)(values);

        if (!isConstructorValid) throw new InvalidOperationException("Invalid constructor - only one constructor must be found.");
        if (!valuesMatchConstructor) throw new ArgumentException("Invalid values for constructor.");

        var obj = ConstructorCache.GetOrAdd(type, _ => Activator<T>.CreateWithValues);

        var objectInstance = (Func<object?[], T>)obj(values);

        return objectInstance.Invoke(values);
    }

    private static class ArrowFieldNamesFromProperties<T>
    {
        public static readonly string[] _names = GetNamesByConstructorOrder(typeof(T));

        private static string[] GetNamesByConstructorOrder(Type type)
        {
            var fields = type.GetProperties().SelectMany(p => p.GetCustomAttributes<ArrowFieldAttribute>());
            return fields.OrderBy(f => f.ConstructorOrder).Select(f => f.Name).ToArray();
        }
    }

    private static class Activator<T>
    {
        public static readonly Func<bool> ValidateConstructor = ValidateConstructorForType();

        private static Func<bool> ValidateConstructorForType()
        {
            var type = typeof(T);
            var ctor = type.GetConstructors();
            return () => ctor.Length == 1;
        }

        public static readonly Func<object?[], bool> ValidateValues = ValidateValuesForConstructor();

        private static Func<object?[], bool> ValidateValuesForConstructor()
        {
            var type = typeof(T);
            var ctor = type.GetConstructors().First();
            var parameters = ctor.GetParameters();
            if (parameters.Length == 0) return _ => false;

            var param = Expression.Parameter(typeof(object?[]), "args");
            var checks = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var index = Expression.Constant(i);
                var parameterType = parameters[i].ParameterType;
                var accessor = Expression.ArrayIndex(param, index);

                if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) == null)
                {
                    checks[i] = Expression.TypeIs(accessor, parameterType);
                }
                else
                {
                    checks[i] = Expression.OrElse(
                        Expression.Equal(accessor, Expression.Constant(null)),
                        Expression.TypeIs(accessor, parameterType));
                }
            }

            var allChecks = checks.Aggregate(Expression.AndAlso);
            var lambda = Expression.Lambda<Func<object?[], bool>>(allChecks, param);
            return lambda.Compile();
        }

        public static readonly Func<object?[], T> CreateWithValues = BuildExpressionForObjectCreation();

        private static Func<object?[], T> BuildExpressionForObjectCreation()
        {
            var type = typeof(T);
            var ctor = type.GetConstructors().First();
            var parameters = ctor.GetParameters();
            var param = Expression.Parameter(typeof(object?[]), "args");
            var arguments = new Expression[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var index = Expression.Constant(i);
                var parameterType = parameters[i].ParameterType;
                var accessor = Expression.ArrayIndex(param, index);
                var cast = Expression.Convert(accessor, parameterType);
                arguments[i] = cast;
            }

            var newExpression = Expression.New(ctor, arguments);
            var lambda = Expression.Lambda<Func<object?[], T>>(newExpression, param);
            return lambda.Compile();
        }
    }
}
