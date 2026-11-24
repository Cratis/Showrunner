// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.ResumePart.when_resuming_a_part;

/// <summary>
/// Resuming a part that is suspended at that level records the decision.
/// </summary>
public class a_suspended_part : a_guest_list_command<ResumePart>
{
    void Establish() => Given(Guests.TenantSuspension);

    async Task Because() => _result = await _scenario.Execute(new ResumePart(Guests.Product, SuspensionLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.FleetReports));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_append_that_it_was_resumed() =>
        await _scenario.ShouldHaveAppendedEvent<ResumePart, PartResumeDecided>(
            new EventSourceId(Guests.Product.Value),
            e => e.Level == SuspensionLevel.Tenant && e.TenantId == Guests.Acme && e.Part == Guests.FleetReports);
}
#endif
