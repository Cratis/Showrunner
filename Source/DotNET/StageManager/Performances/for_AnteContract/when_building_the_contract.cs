// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_AnteContract;

public class when_building_the_contract : Specification
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
        _variables = AnteContract.EnvironmentVariables(_performance);
        _files = AnteContract.SecretFiles(_performance, PerformanceSamples.Environment(), _keys);
    }

    [Fact] void should_enable_onboarding() => _variables["Ante__Onboarding__Enabled"].ShouldEqual("true");
    [Fact] void should_observe_the_first_application_store() => _variables["Ante__Onboarding__HostStore"].ShouldEqual("Crm");
    [Fact] void should_have_its_own_store() => _variables["Ante__Onboarding__EventStore"].ShouldEqual("ante-crm");
    [Fact] void should_use_the_public_address() => _variables["Ante__Onboarding__PublicBaseUrl"].ShouldEqual("https://crm.example.com");
    [Fact] void should_allow_self_service_by_default() => _variables["Ante__Onboarding__RegistrationEnabled"].ShouldEqual("true");
    [Fact] void should_list_the_personas() => _variables["Ante__Onboarding__Personas__1"].ShouldEqual("Member");
    [Fact] void should_read_the_legal_terms_from_the_mounted_folder() => _variables["Ante__Onboarding__Legal__Folder"].ShouldEqual("/etc/ante/legal");
    [Fact] void should_read_secrets_from_the_mounted_folder() => _variables["Ante__Onboarding__Secrets__Path"].ShouldEqual("/run/secrets/ante");
    [Fact] void should_never_enable_development_mode() => _variables.Keys.Any(key => key.Contains("Development", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_use_attested_exchange_when_providers_are_given() => _variables["Ante__Invitations__Exchange__Mode"].ShouldEqual("Attested");
    [Fact] void should_trust_the_attestation_issuer_of_authproxy() => _variables["Ante__Invitations__Exchange__Attestation__Issuer"].ShouldEqual("urn:cratis:authproxy:crm");
    [Fact] void should_keep_no_secret_in_the_variables() => PerformanceSamples.SecretValues.Append(_keys.Invitation.Current.PrivateKeyPem).Any(secret => _variables.Values.Any(value => value.Contains(secret))).ShouldBeFalse();
    [Fact] void should_write_the_signing_key_as_a_file_named_like_the_setting() => _files["Ante__Invitations__Token__PrivateKeyPem"].ShouldEqual(_keys.Invitation.Current.PrivateKeyPem);
    [Fact] void should_write_the_mongodb_server_as_a_file() => _files["Cratis__MongoDB__Server"].ShouldEqual(PerformanceSamples.MongoServer);
    [Fact] void should_pin_the_current_attestation_public_key() => _files["Ante__Invitations__Exchange__Attestation__PublicKeys__0__KeyId"].ShouldEqual(_keys.Attestation.Current.KeyId);
    [Fact] void should_never_hand_ante_the_attestation_private_key() => _files.Values.Contains(_keys.Attestation.Current.PrivateKeyPem).ShouldBeFalse();
}
#endif
