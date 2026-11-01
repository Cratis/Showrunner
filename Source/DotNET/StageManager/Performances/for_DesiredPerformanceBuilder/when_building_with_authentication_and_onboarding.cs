// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_DesiredPerformanceBuilder;

public class when_building_with_authentication_and_onboarding : Specification
{
    DesiredPerformance _desired;
    SigningKeyState _keys;
    string[] _environmentValues;

    IEnumerable<V1Container> Containers => _desired.Deployments.SelectMany(deployment => deployment.Spec.Template.Spec.Containers);

    void Establish()
    {
        _keys = PerformanceSamples.Keys();
        _environmentValues = [.. PerformanceSamples.Environment().Values.Values, .. PerformanceSamples.SecretValues, _keys.Invitation.Current.PrivateKeyPem, _keys.Attestation.Current.PrivateKeyPem];
    }

    void Because() => _desired = DesiredPerformanceBuilder.Build(PerformanceSamples.Performance(), new("chronicle-connection", "connectionString"), PerformanceSamples.Environment(), _keys);

    [Fact] void should_have_a_deployment_for_each_component() => _desired.Deployments.Count.ShouldEqual(3);
    [Fact] void should_have_a_service_for_each_component() => _desired.Services.Count.ShouldEqual(3);
    [Fact] void should_have_a_secret_for_each_component() => _desired.Secrets.Count.ShouldEqual(3);
    [Fact] void should_run_exactly_one_ante() => _desired.Deployments.Count(deployment => deployment.Metadata.Name == "crm-ante").ShouldEqual(1);
    [Fact] void should_run_ante_with_a_single_replica() => _desired.Deployments.Single(deployment => deployment.Metadata.Name == "crm-ante").Spec.Replicas.ShouldEqual(1);
    [Fact] void should_keep_every_service_inside_the_cluster() => _desired.Services.All(service => service.Spec.Type == "ClusterIP").ShouldBeTrue();
    [Fact] void should_route_every_host_to_authproxy_only() => _desired.Ingress!.Spec.Rules.All(rule => rule.Http.Paths.All(path => path.Backend.Service.Name == "crm-authproxy")).ShouldBeTrue();
    [Fact] void should_ask_cert_manager_for_a_certificate() => _desired.Ingress!.Metadata.Annotations["cert-manager.io/cluster-issuer"].ShouldEqual("letsencrypt");
    [Fact] void should_terminate_tls_for_the_host() => _desired.Ingress!.Spec.Tls.Single().Hosts.Single().ShouldEqual("crm.example.com");
    [Fact] void should_hand_no_resolved_value_to_a_container_as_an_environment_variable() => Containers.SelectMany(container => container.Env).Any(variable => _environmentValues.Any(value => variable.Value?.Contains(value, StringComparison.Ordinal) == true)).ShouldBeFalse();
    [Fact] void should_use_no_secret_references_in_environment_variables() => Containers.SelectMany(container => container.Env).All(variable => variable.ValueFrom is null).ShouldBeTrue();
    [Fact] void should_mount_every_secret_read_only() => Containers.SelectMany(container => container.VolumeMounts).All(mount => mount.ReadOnlyProperty == true).ShouldBeTrue();
    [Fact] void should_not_mount_the_service_account_token() => _desired.Deployments.All(deployment => deployment.Spec.Template.Spec.AutomountServiceAccountToken == false).ShouldBeTrue();
    [Fact] void should_not_run_privileged() => Containers.All(container => container.SecurityContext.AllowPrivilegeEscalation == false).ShouldBeTrue();
    [Fact] void should_mount_the_connection_secret_as_a_file_without_copying_it() => _desired.Deployments.Single(deployment => deployment.Metadata.Name == "crm-ante").Spec.Template.Spec.Volumes
        .Where(volume => volume.Projected is not null).SelectMany(volume => volume.Projected.Sources).Single(source => source.Secret.Name == "chronicle-connection").Secret.Items.Single().Path.ShouldEqual("Cratis__Chronicle__ConnectionString");
    [Fact] void should_hand_the_application_only_its_own_values() => _desired.Secrets.Single(secret => secret.Metadata.Name == "crm-app-environment").Data.Keys.Contains("authentication.providers.EntraId.clientSecret").ShouldBeFalse();
    [Fact] void should_own_everything_it_creates() => _desired.Deployments.All(deployment => deployment.Metadata.OwnerReferences.Single().Name == "crm").ShouldBeTrue();
    [Fact] void should_roll_the_pods_when_content_changes() => _desired.Deployments.All(deployment => deployment.Spec.Template.Metadata.Annotations.ContainsKey(PerformanceMetadata.ConfigRevisionAnnotation)).ShouldBeTrue();
}
#endif
