// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Link.Protocol.for_ConfigurationAcceptance.when_evaluating;

public class and_it_renews_the_lease_of_the_revision_held : given.a_delivered_configuration
{
    void Because()
    {
        var held = Snapshot(2, _now.AddMinutes(-40), _now.AddMinutes(20));
        _reason = ConfigurationAcceptance.Evaluate(Signed(held with { IssuedAt = _now, ExpiresAt = _now.AddHours(1) }), _key, "prod", held, _now);
    }

    [Fact] void should_accept_it_as_a_renewal() => _reason.ShouldEqual(ConfigurationAcceptanceReason.Renewed);
}
#endif
