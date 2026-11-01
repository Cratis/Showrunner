// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PublishedStageContract;

public class when_building_the_environment : Specification
{
    IReadOnlyDictionary<string, string> _variables;
    IReadOnlyDictionary<string, string> _withoutNotes;

    void Because()
    {
        _variables = PublishedStageContract.EnvironmentVariables(PerformanceSamples.PublishedPerformance(), "/run/secrets/cratis");
        _withoutNotes = PublishedStageContract.EnvironmentVariables(PerformanceSamples.PublishedPerformance(notes: false), "/run/secrets/cratis");
    }

    [Fact] void should_run_stage_in_published_mode() => _variables["Stage__Mode"].ShouldEqual("Published");
    [Fact] void should_read_the_package_from_the_mounted_archive() => _variables["Stage__Published__PackagePath"].ShouldEqual("/stage/package-archive/package.tar.gz");
    [Fact] void should_read_the_storage_connection_from_a_file() => _variables["Stage__Published__Storage__ConnectionStringFile"].ShouldEqual("/run/secrets/cratis/Stage__Published__Storage__ConnectionString");
    [Fact] void should_trust_the_proxy() => _variables["Stage__Published__Authentication__Mode"].ShouldEqual("TrustedProxy");
    [Fact] void should_trust_the_given_proxy_networks() => _variables["Stage__Published__TrustedProxies__0"].ShouldEqual("10.244.0.0/16");
    [Fact] void should_allow_only_the_published_hosts() => _variables["AllowedHosts"].ShouldEqual("crm.example.com");
    [Fact] void should_enable_notes() => _variables["Stage__Published__Notes__Enabled"].ShouldEqual("true");
    [Fact] void should_send_notes_over_http() => _variables["Notes__Transport"].ShouldEqual("Http");
    [Fact] void should_name_the_collector() => _variables["Notes__CollectorUrl"].ShouldEqual("https://collector.example.invalid:8443");
    [Fact] void should_read_the_collector_token_from_a_file() => _variables["Notes__CollectorTokenFile"].ShouldEqual("/run/secrets/notes/token");
    [Fact] void should_name_the_publication() => _variables["Notes__PublicationId"].ShouldEqual("pub-7c1d");
    [Fact] void should_name_the_collector_tenant() => _variables["Notes__CollectorTenantId"].ShouldEqual("acme-corp");
    [Fact] void should_name_the_application_to_notes() => _variables["Notes__ApplicationName"].ShouldEqual("Crm");
    [Fact] void should_not_hold_a_secret_value() => PerformanceSamples.SecretValues.Any(value => _variables.Values.Contains(value)).ShouldBeFalse();
    [Fact] void should_disable_notes_when_not_asked_for() => _withoutNotes["Stage__Published__Notes__Enabled"].ShouldEqual("false");
    [Fact] void should_not_configure_the_notes_transport_without_notes() => _withoutNotes.ContainsKey("Notes__Transport").ShouldBeFalse();
}
#endif
