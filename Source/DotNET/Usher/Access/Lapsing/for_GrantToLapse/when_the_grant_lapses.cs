// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.Lapsing.for_GrantToLapse;

public class when_the_grant_lapses : a_read_model<GrantToLapse>
{
    async Task Because() => await Happened(new ReporterGrantedAccess(Start.AddHours(1)), new AccessGrantLapsed(Start.AddHours(1)));

    [Fact] void should_have_no_grant_to_lapse() => _scenario.Instance.ShouldBeNull();
}
#endif
