// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_the_grant_ends.given;
using Cratis.Showrunner.Usher.Contracts.Access;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_the_grant_ends;

public class and_an_earlier_grant_lapses_after_a_new_one : a_grant_with_sessions
{
    async Task Because() => await GrantEndedWith(new AccessGrantLapsed(new ValidUntil(Now.AddMinutes(-30))));

    [Fact] void should_not_end_the_open_session() => EndsOf(Open).ShouldBeEmpty();
}
#endif
