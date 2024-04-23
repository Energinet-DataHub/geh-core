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

using Energinet.DataHub.Core.App.Common.Abstractions.Users;

namespace Energinet.DataHub.Core.App.Common.Users;

public sealed class UserContext<TUser> : IUserContext<TUser>
    where TUser : class
{
    private TUser? _currentUser;

    public TUser CurrentUser => _currentUser ?? throw new InvalidOperationException("User has not been set, ensure that all required services and middleware have been registered correctly and that you are not in an anonymous context.");

    public void SetCurrentUser(TUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (_currentUser != null)
            throw new InvalidOperationException("User has already been set, cannot set it again!");

        _currentUser = user;
    }
}
