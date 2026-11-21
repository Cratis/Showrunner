// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_the_lease_expired_in_transit : a_direct_link
{
    async Task Because()
    {
        PublishReadiness();
        _direct.ConfigurationLease = TimeSpan.FromSeconds(-1);
        await Pass();
    }

    [Fact] void should_not_apply_the_configuration() => Status.AppliedRevision.ShouldEqual(0);
    [Fact] void should_say_it_expired() => Status.Problems.ShouldContain(problem => problem.Contains("Expired"));
}
#endif
