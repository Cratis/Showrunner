// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Link.Protocol.for_ConfigurationAcceptance.when_evaluating;

public class and_its_lease_expired_in_transit : given.a_delivered_configuration
{
    void Because() => _reason = ConfigurationAcceptance.Evaluate(Signed(Snapshot(2, _now.AddHours(-2), _now.AddSeconds(-1))), _key, "prod", null, _now);

    [Fact] void should_refuse_it_as_expired() => _reason.ShouldEqual(ConfigurationAcceptanceReason.Expired);
}
#endif
