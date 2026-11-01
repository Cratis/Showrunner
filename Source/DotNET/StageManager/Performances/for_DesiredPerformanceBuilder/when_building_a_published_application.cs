// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_DesiredPerformanceBuilder;

public class when_building_a_published_application : Specification
{
    DesiredPerformance _desired;
    V1Deployment _application;
    V1Container _container;

    void Because()
    {
        var performance = PerformanceSamples.PublishedPerformance();
        var published = new PublishedInputs(new("crm-storage", "connectionString"), new("crm-notes-token", "token"), Releases.V140);
        _desired = DesiredPerformanceBuilder.Build(performance, null, PerformanceSamples.Environment(), PerformanceSamples.Keys(), published);
        _application = _desired.Deployments.Single(deployment => deployment.Metadata.Name == "crm-app");
        _container = _application.Spec.Template.Spec.Containers.Single();
    }

    string Env(string name) => _container.Env.Single(variable => variable.Name == name).Value;

    V1Volume Volume(string name) => _application.Spec.Template.Spec.Volumes.Single(volume => volume.Name == name);

    [Fact] void should_run_the_application_and_authproxy_only() => string.Join(',', _desired.Deployments.Select(deployment => deployment.Metadata.Name).Order(StringComparer.Ordinal)).ShouldEqual("crm-app,crm-authproxy");
    [Fact] void should_run_the_stage_runtime_image() => _container.Image.ShouldEqual("cratis/stage:5.3.0");
    [Fact] void should_listen_on_the_stage_port() => _container.Ports.Single().ContainerPort.ShouldEqual(9090);
    [Fact] void should_run_in_published_mode() => Env("Stage__Mode").ShouldEqual("Published");
    [Fact] void should_not_point_at_a_chronicle() => _container.Env.Any(variable => variable.Name.StartsWith("CRATIS_CHRONICLE", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_mount_the_package_configmap() => Volume("package").ConfigMap.Name.ShouldEqual("crm-package-1-4-0");
    [Fact] void should_mount_the_package_read_only() => _container.VolumeMounts.Single(mount => mount.Name == "package").ReadOnlyProperty.ShouldEqual(true);
    [Fact] void should_mount_the_package_where_stage_reads_it() => _container.VolumeMounts.Single(mount => mount.Name == "package").MountPath.ShouldEqual("/stage/package-archive");
    [Fact] void should_project_the_storage_secret_as_a_file() => Volume("environment").Projected.Sources.Any(source => source.Secret.Name == "crm-storage" && source.Secret.Items.Single().Path == "Stage__Published__Storage__ConnectionString").ShouldBeTrue();
    [Fact] void should_project_the_notes_token_as_a_file() => Volume("notes").Projected.Sources.Single().Secret.Items.Single().Path.ShouldEqual("token");
    [Fact] void should_not_put_a_secret_value_in_the_deployment() => PerformanceSamples.SecretValues.Any(value => JsonSerializer.Serialize(_desired.Deployments).Contains(value)).ShouldBeFalse();
    [Fact] void should_run_one_replica() => _application.Spec.Replicas.ShouldEqual(1);
    [Fact] void should_stop_the_running_kernel_before_starting_the_next() => _application.Spec.Strategy.Type.ShouldEqual("Recreate");
    [Fact] void should_be_ready_when_stage_says_so() => _container.ReadinessProbe.HttpGet.Path.ShouldEqual("/stage/status");
    [Fact] void should_request_the_default_cpu() => _container.Resources.Requests["cpu"].ToString().ShouldEqual("250m");
    [Fact] void should_limit_the_default_memory() => _container.Resources.Limits["memory"].ToString().ShouldEqual("1Gi");
    [Fact] void should_restrict_the_application_with_a_network_policy() => _desired.NetworkPolicy!.Metadata.Name.ShouldEqual("crm-app");
    [Fact] void should_still_route_the_ingress_to_authproxy_only() => _desired.Ingress!.Spec.Rules.All(rule => rule.Http.Paths.All(path => path.Backend.Service.Name == "crm-authproxy")).ShouldBeTrue();
    [Fact] void should_keep_the_application_service_inside_the_cluster() => _desired.Services.Single(service => service.Metadata.Name == "crm-app").Spec.Type.ShouldEqual("ClusterIP");
    [Fact] void should_have_no_network_policy_for_a_plain_application() => DesiredPerformanceBuilder.Build(PerformanceSamples.Performance(), new("chronicle-connection", "connectionString"), PerformanceSamples.Environment(), PerformanceSamples.Keys()).NetworkPolicy.ShouldBeNull();
}
#endif
