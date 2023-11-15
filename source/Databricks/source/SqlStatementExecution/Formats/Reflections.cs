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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;

internal static class Reflections
{
    public static string[] GetArrowFieldNames<T>()
        => ArrowFieldNamesFromProperties<T>._names;

    /// <summary>
    /// Creates an instance of <typeparamref name="T" /> using the constructor
    /// </summary>
    /// <param name="values">Values for the constructor</param>
    /// <typeparam name="T">Object type to create</typeparam>
    /// <returns>Hydrated object</returns>
    /// <exception cref="InvalidOperationException">If there is less or more then one constructor</exception>
    /// <exception cref="ArgumentException">The supplied values does not match</exception>
    public static T CreateInstance<T>(params object?[] values)
    {
        var isConstructorValid = Activator<T>.ValidateConstructor();
        var valuesMatchConstructor = Activator<T>.ValidateValues(values);

        if (!isConstructorValid) throw new InvalidOperationException("Invalid constructor - only one constructor must be found.");
        if (!valuesMatchConstructor) throw new ArgumentException("Invalid values for constructor.");

        return Activator<T>.CreateWithValues(values);
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
        /// <summary>
        /// Validates that the type has a single constructor
        /// </summary>
        public static readonly Func<bool> ValidateConstructor = ValidateConstructorForType();

        private static Func<bool> ValidateConstructorForType()
        {
            var type = typeof(T);
            var ctor = type.GetConstructors();

            var ctorCount = Expression.Constant(ctor.Length);
            var expectedCtorCount = Expression.Constant(1);
            var isEqual = Expression.Equal(ctorCount, expectedCtorCount);

            var lambda = Expression.Lambda<Func<bool>>(isEqual);
            return lambda.Compile();
        }

        /// <summary>
        /// Validates that the values matches the constructor
        /// </summary>
        public static readonly Func<object?[], bool> ValidateValues = ValidateValuesForConstructor();

        private static Func<object?[], bool> ValidateValuesForConstructor()
        {
            var type = typeof(T);
            var ctor = type.GetConstructors().First();
            var parameters = ctor.GetParameters();
            if (parameters.Length == 0) return _ => false; // No parameters to validate - false

            var param = Expression.Parameter(typeof(object?[]), "args");
            var checks = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var index = Expression.Constant(i);
                var parameterType = parameters[i].ParameterType;
                var accessor = Expression.ArrayIndex(param, index);
                var typeMatch = Expression.TypeIs(accessor, parameterType);
                checks[i] = typeMatch;
            }

            var allChecks = checks.Aggregate(Expression.AndAlso);

            var lambda = Expression.Lambda<Func<object?[], bool>>(allChecks, param);
            return lambda.Compile();
        }

        /// <summary>
        ///  Creates an instance of <typeparamref name="T" /> using the constructor
        /// </summary>
        /// <exception cref="InvalidOperationException">is thrown if <typeparamref name="T"/> contains more then one constructor</exception>
        public static Func<object?[], T> CreateWithValues =>
            typeof(T).GetConstructors().Length > 1 ?
                throw new InvalidOperationException("Only one constructor is supported.") :
                BuildExpressionForObjectCreation();

        /// <summary>
        ///  Builds an expression that creates an instance of <typeparamref name="T" /> using the constructor
        /// </summary>
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
