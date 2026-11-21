// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_direct_presents_another_signing_key : a_direct_link
{
    string _pinned;

    async Task Because()
    {
        PublishReadiness();
        await Pass();
        _pinned = Identity.SigningKey;
        _direct.RotateKey();
        await Pass();
    }

    [Fact] void should_refuse_to_trust_it() => Status.Connection.ShouldEqual(LinkConnection.Refused);
    [Fact] void should_keep_the_key_it_pinned() => Identity.SigningKey.ShouldEqual(_pinned);
    [Fact] void should_say_how_to_recover() => Status.Problems.ShouldContain(problem => problem.Contains("pinned"));
}
#endif
