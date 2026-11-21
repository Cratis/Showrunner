// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Link.Protocol.for_ConfigurationAcceptance.when_evaluating;

public class and_it_is_the_revision_held_with_other_content : given.a_delivered_configuration
{
    void Because()
    {
        var held = Snapshot(2, _now.AddMinutes(-40), _now.AddMinutes(20));
        var changed = held with { Offline = new OfflinePolicy(true, true), IssuedAt = _now, ExpiresAt = _now.AddHours(1) };
        _reason = ConfigurationAcceptance.Evaluate(Signed(changed with { Digest = ConfigurationDigest.Of(changed) }), _key, "prod", held, _now);
    }

    [Fact] void should_refuse_it_so_a_renewal_can_never_change_a_grant() => _reason.ShouldEqual(ConfigurationAcceptanceReason.NotNewer);
}
#endif
