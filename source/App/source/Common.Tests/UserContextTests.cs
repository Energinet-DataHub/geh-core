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
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests
{
    public sealed class UserContextTests
    {
        [Fact]
        public void CurrentUser_NoUser_ThrowsException()
        {
            // Arrange
            var userContext = new UserContext<object>();

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => userContext.CurrentUser);
        }

        [Fact]
        public void CurrentUser_HasUser_ReturnsUser()
        {
            // Arrange
            var domainUser = new object();
            var userContext = new UserContext<object>();
            userContext.SetCurrentUser(domainUser);

            // Act
            var currentUser = userContext.CurrentUser;

            // Assert
            Assert.Equal(domainUser, currentUser);
        }

        [Fact]
        public void SetCurrentUser_HasUser_ThrowsException()
        {
            // Arrange
            var domainUser = new object();
            var userContext = new UserContext<object>();
            userContext.SetCurrentUser(domainUser);

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => userContext.SetCurrentUser(new object()));
        }

        [Fact]
        public void SetCurrentUser_SetNullUser_ThrowsException()
        {
            // Arrange
            var userContext = new UserContext<object>();

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => userContext.SetCurrentUser(null!));
        }
    }
}
