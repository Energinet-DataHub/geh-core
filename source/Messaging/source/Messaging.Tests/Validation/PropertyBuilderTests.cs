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
using System.Linq.Expressions;
using Energinet.DataHub.Core.Messaging.Tests.TestHelpers;
using Energinet.DataHub.Core.Messaging.Tests.TestHelpers.Validation;
using Energinet.DataHub.Core.Messaging.Validation;
using FluentAssertions;
using FluentValidation;
using Xunit;
using Xunit.Categories;
using ChangeOfSupplier = Energinet.DataHub.Core.Messaging.Tests.TestHelpers.ChangeOfSupplier;

namespace Energinet.DataHub.Core.Messaging.Tests.Validation
{
    [UnitTest]
    public class PropertyBuilderTests
    {
        [Fact]
        public void PropertyRuleShouldBeAssigned()
        {
            // Arrange
            Expression<Func<ChangeOfSupplier, string?>> selector = cos => cos.MarketEvaluationPoint;
            var tracking = new List<Action<ServiceProviderDelegate, AbstractValidator<ChangeOfSupplier>>>();

            var builder = new PropertyBuilder<ChangeOfSupplier, string?>(
                selector,
                tracking);

            // Act
            builder.PropertyRule<MarketEvaluationPointValidation>();

            // Assert
            tracking.Should().HaveCount(1);
        }

        [Fact]
        public void RuleCollectionShouldBeAssigned()
        {
            // Arrange
            Expression<Func<MarketParticipant, string>> selector = mp => mp.Name!;
            var tracking = new List<Action<ServiceProviderDelegate, AbstractValidator<MarketParticipant>>>();

            var builder = new PropertyBuilder<MarketParticipant, string>(selector, tracking);

            // Act
            builder.RuleCollection<MarketParticipantNameRuleCollection>();

            // Assert
            tracking.Should().HaveCount(1);
        }
    }
}
