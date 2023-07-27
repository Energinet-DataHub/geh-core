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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Energinet.DataHub.Core.SqlStatement;

public interface ISqlStatementClient
{
    /// <summary>
    /// The method will execute the given sql query and return the result as a list of <typeparamref name="TModel"/>
    /// </summary>
    /// <typeparam name="TModel">The model that should be returned</typeparam>
    /// <param name="sqlQuery"></param>
    /// <param name="mapResult"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<List<TModel>> GetAsync<TModel>(string sqlQuery, Func<List<string>, TModel> mapResult);
}
