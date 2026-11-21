// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.for_StageManagerSecrets;

public class when_protecting_credentials : Specification
{
    bool _plainValueIsSecret;
    bool _outputIsSecret;

    async Task Because()
    {
        _plainValueIsSecret = await Output.IsSecretAsync(StageManagerSecrets.Protect("plain-credential"));
        _outputIsSecret = await Output.IsSecretAsync(StageManagerSecrets.Protect(Output.Create("credential-from-another-resource")));
    }

    [Fact] void should_make_a_plain_value_a_pulumi_secret() => _plainValueIsSecret.ShouldBeTrue();
    [Fact] void should_make_the_output_of_another_resource_a_pulumi_secret() => _outputIsSecret.ShouldBeTrue();
}
#endif
