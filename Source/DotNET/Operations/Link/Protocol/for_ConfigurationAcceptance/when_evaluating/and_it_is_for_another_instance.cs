// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Link.Protocol.for_ConfigurationAcceptance.when_evaluating;

public class and_it_is_for_another_instance : given.a_delivered_configuration
{
    void Because() => _reason = ConfigurationAcceptance.Evaluate(Signed(Snapshot(2, _now, _now.AddHours(1), "other")), _key, "prod", null, _now);

    [Fact] void should_refuse_it_as_the_wrong_instance() => _reason.ShouldEqual(ConfigurationAcceptanceReason.WrongInstance);
}
#endif
