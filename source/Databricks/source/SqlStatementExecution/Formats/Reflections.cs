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
        => Column<T>._names;

    public static T CreateInstance<T>(params object?[] values)
        => Creator<T>._create(values);

    private static class Column<T>
    {
        public static readonly string[] _names = GetNames(typeof(T));

        private static string[] GetNames(Type type)
        {
            var fields = type.GetProperties().SelectMany(p => p.GetCustomAttributes<ArrowFieldAttribute>());

            return fields.OrderBy(f => f.ConstructorOrder).Select(f => f.Name).ToArray();
        }
    }

    private static class Creator<T>
    {
        public static readonly Func<object?[], T> _create = CreateCreator();

        private static Func<object?[], T> CreateCreator()
        {
            var type = typeof(T);
            var ctor = type.GetConstructors().Single();
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
