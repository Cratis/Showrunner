// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Link.Protocol.for_ConfigurationAcceptance.when_evaluating;

public class and_it_is_newer_signed_and_unexpired : given.a_delivered_configuration
{
    void Because() => _reason = ConfigurationAcceptance.Evaluate(Signed(_snapshot), _key, "prod", Snapshot(1, _now.AddHours(-1), _now.AddMinutes(30)), _now);

    [Fact] void should_accept_it() => _reason.ShouldEqual(ConfigurationAcceptanceReason.Accepted);
}
#endif
