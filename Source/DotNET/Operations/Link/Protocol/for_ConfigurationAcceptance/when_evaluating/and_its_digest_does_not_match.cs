// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Link.Protocol.for_ConfigurationAcceptance.when_evaluating;

public class and_its_digest_does_not_match : given.a_delivered_configuration
{
    void Because() => _reason = ConfigurationAcceptance.Evaluate(Signed(_snapshot with { Digest = "tampered" }), _key, "prod", null, _now);

    [Fact] void should_refuse_it_as_a_digest_mismatch() => _reason.ShouldEqual(ConfigurationAcceptanceReason.DigestMismatch);
}
#endif
