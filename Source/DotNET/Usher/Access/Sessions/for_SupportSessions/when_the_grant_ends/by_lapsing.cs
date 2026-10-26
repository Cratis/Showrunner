// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_the_grant_ends.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_the_grant_ends;

public class by_lapsing : a_grant_with_sessions
{
    async Task Because() => await GrantEndedWith(new AccessGrantLapsed(ValidUntil));

    [Fact] void should_end_the_open_session() => EndsOf(Open).Count().ShouldEqual(1);
}
#endif
