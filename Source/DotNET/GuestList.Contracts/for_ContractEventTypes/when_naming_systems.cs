// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Contracts.Audiences;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Contracts.for_ContractEventTypes;

/// <summary>
/// Naming a system changed the shape of the audience events, which had shipped, so each is a new generation of the same event
/// type, and the generation that shipped is kept: a stored or published event of generation 1 is still read, and means the
/// product-wide system holder.
/// </summary>
public class when_naming_systems : Specification
{
    [Fact] void should_keep_the_identity_of_the_grant() => Assert.Equal(typeof(AudienceGrantedV1).GetEventType().Id, typeof(AudienceGranted).GetEventType().Id);
    [Fact] void should_make_the_grant_generation_2() => Assert.Equal(2u, typeof(AudienceGranted).GetEventType().Generation.Value);
    [Fact] void should_keep_the_first_generation_of_the_grant() => Assert.Equal(1u, typeof(AudienceGrantedV1).GetEventType().Generation.Value);
    [Fact] void should_keep_the_identity_of_the_withdrawal() => Assert.Equal(typeof(AudienceWithdrawnV1).GetEventType().Id, typeof(AudienceWithdrawn).GetEventType().Id);
    [Fact] void should_make_the_withdrawal_generation_2() => Assert.Equal(2u, typeof(AudienceWithdrawn).GetEventType().Generation.Value);
    [Fact] void should_keep_the_first_generation_of_the_withdrawal() => Assert.Equal(1u, typeof(AudienceWithdrawnV1).GetEventType().Generation.Value);
    [Fact] void should_keep_the_identity_of_the_receipt() => Assert.Equal(typeof(AudienceDecisionReceivedV1).GetEventType().Id, typeof(AudienceDecisionReceived).GetEventType().Id);
    [Fact] void should_make_the_receipt_generation_2() => Assert.Equal(2u, typeof(AudienceDecisionReceived).GetEventType().Generation.Value);
    [Fact] void should_keep_the_first_generation_of_the_receipt() => Assert.Equal(1u, typeof(AudienceDecisionReceivedV1).GetEventType().Generation.Value);
    [Fact] void should_report_the_systems_as_a_new_event_of_the_first_generation() => Assert.Equal(1u, typeof(SystemsDeclared).GetEventType().Generation.Value);
    [Fact] void should_keep_the_levels_that_shipped_where_they_were() => Assert.Equal([0, 1, 2], new[] { (int)AudienceLevel.Tenant, (int)AudienceLevel.User, (int)AudienceLevel.System });
    [Fact] void should_add_the_named_system_after_them() => Assert.Equal(3, (int)AudienceLevel.NamedSystem);
}
#endif
