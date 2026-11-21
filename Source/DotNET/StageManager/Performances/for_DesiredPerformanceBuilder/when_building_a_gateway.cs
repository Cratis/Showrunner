// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_DesiredPerformanceBuilder;

public class when_building_a_gateway : Specification
{
    DesiredPerformance _desired;
    V1Deployment _proxy;
    string[] _values;

    void Establish() => _values = [ProxySamples.GitHubClientSecret];

    void Because()
    {
        _desired = DesiredPerformanceBuilder.Build(ProxySamples.Gateway(), null, ProxySamples.CredentialsEnvironment(), PerformanceSamples.Keys());
        _proxy = _desired.Deployments.Single();
    }

    [Fact] void should_run_only_authproxy() => _proxy.Metadata.Name.ShouldEqual("console-authproxy");
    [Fact] void should_have_one_service() => _desired.Services.Count.ShouldEqual(1);
    [Fact] void should_have_one_secret() => _desired.Secrets.Count.ShouldEqual(1);
    [Fact] void should_not_isolate_an_application_it_does_not_run() => _desired.EgressPolicy.ShouldBeNull();
    [Fact] void should_route_every_host_to_authproxy() => _desired.Ingress!.Spec.Rules.All(rule => rule.Http.Paths.All(path => path.Backend.Service.Name == "console-authproxy")).ShouldBeTrue();
    [Fact] void should_keep_the_annotation_of_the_ingress() => _desired.Ingress!.Metadata.Annotations["nginx.ingress.kubernetes.io/proxy-read-timeout"].ShouldEqual("3600");
    [Fact] void should_keep_the_annotation_of_the_issuer() => _desired.Ingress!.Metadata.Annotations["cert-manager.io/cluster-issuer"].ShouldEqual("letsencrypt");
    [Fact] void should_mount_the_pages_as_a_folder() => _proxy.Spec.Template.Spec.Containers[0].VolumeMounts.Single(mount => mount.Name == "pages").MountPath.ShouldEqual("/mnt/pages");
    [Fact] void should_read_the_pages_from_their_config_map() => _proxy.Spec.Template.Spec.Volumes.Single(volume => volume.Name == "pages").ConfigMap.Name.ShouldEqual("console-pages");
    [Fact] void should_mount_the_key_ring() => _proxy.Spec.Template.Spec.Containers[0].VolumeMounts.Single(mount => mount.Name == "keys").MountPath.ShouldEqual("/keys");
    [Fact] void should_keep_the_key_ring_on_the_claim() => _proxy.Spec.Template.Spec.Volumes.Single(volume => volume.Name == "keys").PersistentVolumeClaim.ClaimName.ShouldEqual("console-keys");
    [Fact] void should_stop_before_it_rolls_out_because_the_claim_is_read_write_once() => _proxy.Spec.Strategy.Type.ShouldEqual("Recreate");
    [Fact] void should_request_the_resources() => _proxy.Spec.Template.Spec.Containers[0].Resources.Requests["cpu"].ToString().ShouldEqual("100m");
    [Fact] void should_not_put_the_client_secret_in_a_deployment() => _desired.Deployments.SelectMany(deployment => deployment.Spec.Template.Spec.Containers).SelectMany(container => container.Env).Any(variable => _values.Any(value => variable.Value?.Contains(value) == true)).ShouldBeFalse();
    [Fact] void should_hand_the_client_secret_to_authproxy_in_its_secret() => _desired.Secrets.Single().Data.ContainsKey("authentication.providers.GitHub.clientSecret").ShouldBeTrue();
}
#endif
