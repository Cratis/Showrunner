// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ApplicationObjectBuilder;

public class when_notes_is_sent_over_http : Specification
{
    DesiredObjects _result = null!;
    DesiredObjects _withoutCollector = null!;
    DesiredObjects _disabled = null!;

    Dictionary<string, string> Env(DesiredObjects objects) => objects.Deployment.Spec.Template.Spec.Containers[0].Env.ToDictionary(e => e.Name, e => e.Value);

    void Because()
    {
        var application = Applications.Create();
        application.Spec = application.Spec with { Notes = new StageApplicationNotes { Enabled = true, CollectorUrl = "https://collector.example.invalid:8443", CollectorTokenSecretRef = "orders-notes-token" } };
        _result = ApplicationObjectBuilder.Build(application, Applications.NewImage, 1, true, new StageOperatorOptions());

        var plain = Applications.Create();
        plain.Spec = plain.Spec with { Notes = new StageApplicationNotes { Enabled = true } };
        _withoutCollector = ApplicationObjectBuilder.Build(plain, Applications.NewImage, 1, true, new StageOperatorOptions());

        var disabled = Applications.Create();
        disabled.Spec = disabled.Spec with { Notes = new StageApplicationNotes { Enabled = false, CollectorUrl = "https://collector.example.invalid:8443", CollectorTokenSecretRef = "orders-notes-token" } };
        _disabled = ApplicationObjectBuilder.Build(disabled, Applications.NewImage, 1, true, new StageOperatorOptions());
    }

    [Fact] void should_use_the_http_transport() => Assert.Equal("Http", Env(_result)["Notes__Transport"]);
    [Fact] void should_name_the_collector() => Assert.Equal("https://collector.example.invalid:8443", Env(_result)["Notes__CollectorUrl"]);
    [Fact] void should_read_the_token_from_the_mounted_file() => Assert.Equal("/stage/notes/token", Env(_result)["Notes__CollectorTokenFile"]);
    [Fact] void should_identify_the_publication() => Assert.Equal("22222222-2222-2222-2222-222222222222", Env(_result)["Notes__PublicationId"]);
    [Fact] void should_name_the_tenant_after_the_organization() => Assert.Equal("Example Org", Env(_result)["Notes__CollectorTenantId"]);
    [Fact] void should_name_the_application_after_the_published_name() => Assert.Equal("orders", Env(_result)["Notes__ApplicationName"]);
    [Fact] void should_mount_the_secret_read_only_with_only_the_token_key() => Assert.Equal(("orders-notes-token", "token", "/stage/notes", true), (_result.Deployment.Spec.Template.Spec.Volumes.Single().Secret.SecretName, _result.Deployment.Spec.Template.Spec.Volumes.Single().Secret.Items.Single().Key, _result.Deployment.Spec.Template.Spec.Containers[0].VolumeMounts.Single().MountPath, _result.Deployment.Spec.Template.Spec.Containers[0].VolumeMounts.Single().ReadOnlyProperty));
    [Fact] void should_not_put_the_token_in_the_environment() => Assert.DoesNotContain(_result.Deployment.Spec.Template.Spec.Containers[0].Env, e => e.Name == "Notes__CollectorToken");
    [Fact] void should_leave_egress_unrestricted() => Assert.DoesNotContain("Egress", _result.NetworkPolicies[0].Spec.PolicyTypes);
    [Fact] void should_not_configure_the_transport_without_a_collector() => Assert.DoesNotContain("Notes__Transport", Env(_withoutCollector).Keys);
    [Fact] void should_not_configure_the_transport_when_notes_is_disabled() => Assert.Null(_disabled.Deployment.Spec.Template.Spec.Volumes);
}
#endif
