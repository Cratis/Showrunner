// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Contracts.Audiences;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Contracts.for_ContractEventTypes;

/// <summary>
/// The audience events have one generation, the latest. Naming a system is part of that one shape, so there is nothing to
/// migrate between and nothing a reader has to upcast.
/// </summary>
public class when_declaring_the_audience_events : Specification
{
    [Fact] void should_have_one_generation_of_the_grant() => Assert.Equal(1u, typeof(AudienceGranted).GetEventType().Generation.Value);
    [Fact] void should_have_one_generation_of_the_withdrawal() => Assert.Equal(1u, typeof(AudienceWithdrawn).GetEventType().Generation.Value);
    [Fact] void should_have_one_generation_of_the_receipt() => Assert.Equal(1u, typeof(AudienceDecisionReceived).GetEventType().Generation.Value);
    [Fact] void should_report_the_systems_as_an_event_of_the_first_generation() => Assert.Equal(1u, typeof(SystemsDeclared).GetEventType().Generation.Value);
    [Fact] void should_not_declare_any_migration() => Assert.DoesNotContain(typeof(AudienceGranted).Assembly.GetTypes(), type => type.Name.EndsWith("Migration", StringComparison.Ordinal));
    [Fact] void should_keep_the_levels_that_shipped_where_they_were() => Assert.Equal([0, 1, 2], new[] { (int)AudienceLevel.Tenant, (int)AudienceLevel.User, (int)AudienceLevel.System });
    [Fact] void should_add_the_named_system_after_them() => Assert.Equal(3, (int)AudienceLevel.NamedSystem);
}
#endif
