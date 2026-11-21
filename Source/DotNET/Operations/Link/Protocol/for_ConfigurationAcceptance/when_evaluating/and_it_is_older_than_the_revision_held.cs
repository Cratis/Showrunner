// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Link.Protocol.for_ConfigurationAcceptance.when_evaluating;

public class and_it_is_older_than_the_revision_held : given.a_delivered_configuration
{
    void Because() => _reason = ConfigurationAcceptance.Evaluate(Signed(Snapshot(1, _now, _now.AddHours(1))), _key, "prod", Snapshot(2, _now.AddHours(-1), _now.AddMinutes(30)), _now);

    [Fact] void should_refuse_it_as_not_newer() => _reason.ShouldEqual(ConfigurationAcceptanceReason.NotNewer);
}
#endif
