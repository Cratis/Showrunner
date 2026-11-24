// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.ResumePart.when_resuming_a_part;

/// <summary>
/// A caller the host's authoring policy does not admit may not resume.
/// </summary>
public class a_caller_that_is_not_an_operator : a_guest_list_command<ResumePart>
{
    protected override ClaimsPrincipal Principal => Operators.Somebody;

    void Establish() => Given(Guests.TenantSuspension);

    async Task Because() => _result = await _scenario.Execute(new ResumePart(Guests.Product, SuspensionLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.FleetReports));

    [Fact] void should_not_be_authorized() => _result.IsAuthorized.ShouldBeFalse();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
