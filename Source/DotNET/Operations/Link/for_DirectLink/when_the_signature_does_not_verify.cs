// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_the_signature_does_not_verify : a_direct_link
{
    async Task Because()
    {
        PublishReadiness();
        _direct.TamperSignature = true;
        await Pass();
    }

    [Fact] void should_not_apply_the_configuration() => Status.AppliedRevision.ShouldEqual(0);
    [Fact] void should_say_why() => Status.Problems.ShouldContain(problem => problem.Contains("InvalidSignature"));
    [Fact] void should_not_run_any_check_it_defines() => _endpoints.Requests.ShouldBeEmpty();
    [Fact] void should_not_acknowledge_it() => _direct.Acknowledged.ShouldBeEmpty();
}
#endif
