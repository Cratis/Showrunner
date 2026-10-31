// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Queries;
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.Announcements.for_Announcement.when_observing_announcements;

/// <summary>
/// A caller the host's authoring policy does not admit sees nothing of what is being authored.
/// </summary>
public class a_caller_that_is_not_an_operator : an_authoring_query
{
    protected override ClaimsPrincipal Principal => Operators.Somebody;

    async Task Because() => _result = await _scenario.Perform(nameof(Announcement.ForProduct), new QueryArguments { ["product"] = "ledger" });

    [Fact] void should_not_be_authorized() => _result.IsAuthorized.ShouldBeFalse();
}
#endif
