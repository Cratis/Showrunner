// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_AuthProxyContract;

public class when_building_the_contract_for_a_performance_with_onboarding : Specification
{
    Performance _performance;
    SigningKeyState _keys;
    IReadOnlyDictionary<string, string> _variables;
    IReadOnlyDictionary<string, string> _files;

    void Establish()
    {
        _performance = PerformanceSamples.Performance();
        _keys = PerformanceSamples.Keys();
    }

    void Because()
    {
        _variables = AuthProxyContract.EnvironmentVariables(_performance);
        _files = AuthProxyContract.SecretFiles(_performance, PerformanceSamples.Environment(), _keys);
    }

    [Fact] void should_declare_the_first_provider() => _variables["Cratis__AuthProxy__Application__Providers__0__Kind"].ShouldEqual("EntraId");
    [Fact] void should_declare_the_second_provider_identity() => _variables["Cratis__AuthProxy__Application__Providers__1__Identity"].ShouldEqual("GitHub");
    [Fact] void should_never_run_in_development_mode() => _variables["Cratis__AuthProxy__Application__Development"].ShouldEqual("false");
    [Fact] void should_turn_onboarding_on() => _variables["Cratis__AuthProxy__Application__Onboarding"].ShouldEqual("true");
    [Fact] void should_name_the_environment() => _variables["Cratis__AuthProxy__Application__EnvironmentName"].ShouldEqual("production");
    [Fact] void should_point_at_the_mounted_secrets_directory() => _variables["Cratis__AuthProxy__Application__SecretsDirectory"].ShouldEqual("/run/secrets/cratis");
    [Fact] void should_not_carry_environment_values_as_variables() => PerformanceSamples.SecretValues.Any(secret => _variables.Values.Any(value => value.Contains(secret))).ShouldBeFalse();
    [Fact] void should_not_carry_the_environment_as_variables() => _variables.Keys.Any(key => key.Contains("__Environment__")).ShouldBeFalse();
    [Fact] void should_write_the_client_secret_as_a_file_named_like_its_key() => _files["authentication.providers.EntraId.clientSecret"].ShouldEqual(PerformanceSamples.EntraClientSecret);
    [Fact] void should_leave_out_providers_that_are_not_declared() => _files.ContainsKey("authentication.providers.Unused.clientSecret").ShouldBeFalse();
    [Fact] void should_leave_out_keys_that_belong_to_neither_provider_nor_onboarding() => _files.ContainsKey("smtp.host").ShouldBeFalse();
    [Fact] void should_leave_out_ante_keys() => _files.ContainsKey("ante.mongodb.server").ShouldBeFalse();
    [Fact] void should_derive_the_stage_url_from_the_public_address() => _files["onboarding.stageUrl"].ShouldEqual("https://crm.example.com/_invite/stage");
    [Fact] void should_derive_the_token_issuer_from_antes_own_store() => _files["onboarding.issuer"].ShouldEqual("urn:cratis:ante:ante-crm:Default");
    [Fact] void should_use_the_generated_attestation_key_id() => _files["onboarding.attestation.keyId"].ShouldEqual(_keys.Attestation.Current.KeyId);
    [Fact] void should_use_the_generated_attestation_private_key() => _files["onboarding.attestation.privateKeyPem"].ShouldEqual(_keys.Attestation.Current.PrivateKeyPem);
    [Fact] void should_not_hand_authproxy_antes_signing_key() => _files.Values.Contains(_keys.Invitation.Current.PrivateKeyPem).ShouldBeFalse();
}
#endif
