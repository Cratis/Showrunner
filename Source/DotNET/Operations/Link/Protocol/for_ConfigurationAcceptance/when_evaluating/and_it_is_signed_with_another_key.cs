// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.Link.Protocol.for_ConfigurationAcceptance.when_evaluating;

public class and_it_is_signed_with_another_key : given.a_delivered_configuration
{
    void Because() => _reason = ConfigurationAcceptance.Evaluate(Signed(_snapshot, ConfigurationSignature.NewKey()), _key, "prod", null, _now);

    [Fact] void should_refuse_it_as_an_invalid_signature() => _reason.ShouldEqual(ConfigurationAcceptanceReason.InvalidSignature);
}
#endif
