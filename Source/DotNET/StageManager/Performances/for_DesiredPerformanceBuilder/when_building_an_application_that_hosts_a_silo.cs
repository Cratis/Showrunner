// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_DesiredPerformanceBuilder;

public class when_building_an_application_that_hosts_a_silo : Specification
{
    V1Deployment _application;
    V1PodSpec _pod;
    V1Container _container;

    void Because()
    {
        var performance = PerformanceSamples.Performance(onboarding: false, authentication: false);
        var application = performance.Spec.Application;
        application.Strategy = ApplicationSpec.Recreate;
        application.ImagePullSecrets = ["registry"];
        application.Probes = new() { ReadinessPath = "/health/ready", LivenessPath = "/health/live", StartupPath = "/health/startup" };
        application.Resources = new()
        {
            Requests = new Dictionary<string, string> { ["cpu"] = "250m", ["memory"] = "512Mi" },
            Limits = new Dictionary<string, string> { ["memory"] = "1Gi" }
        };
        application.NodeSelector = new Dictionary<string, string> { ["pool"] = "core" };
        application.Volumes = [new() { Name = "chronicle-trust" }, new() { Name = "data", Kind = ApplicationVolume.PersistentVolumeClaim, ClaimName = "crm-data" }];
        application.VolumeMounts = [new() { Volume = "chronicle-trust", Path = "/trust", ReadOnly = true }, new() { Volume = "data", Path = "/var/lib/crm" }];
        application.InitContainers =
        [
            new()
            {
                Name = "import-trust",
                Command = ["/bin/sh", "-c", "cp /etc/ssl/certs/* /trust"],
                Environment = new Dictionary<string, string> { ["MODE"] = "copy" },
                VolumeMounts = [new() { Volume = "chronicle-trust", Path = "/trust" }]
            }
        ];

        var desired = DesiredPerformanceBuilder.Build(performance, new("chronicle-connection", "connectionString"), PerformanceSamples.Environment(), PerformanceSamples.Keys());
        _application = desired.Deployments.Single(deployment => deployment.Metadata.Name == "crm-app");
        _pod = _application.Spec.Template.Spec;
        _container = _pod.Containers.Single();
    }

    [Fact] void should_stop_the_old_pod_before_starting_the_new_one() => _application.Spec.Strategy.Type.ShouldEqual("Recreate");
    [Fact] void should_pull_the_image_with_the_secret() => _pod.ImagePullSecrets.Single().Name.ShouldEqual("registry");
    [Fact] void should_select_the_nodes() => _pod.NodeSelector["pool"].ShouldEqual("core");
    [Fact] void should_check_readiness_over_http() => _container.ReadinessProbe.HttpGet.Path.ShouldEqual("/health/ready");
    [Fact] void should_check_liveness_over_http() => _container.LivenessProbe.HttpGet.Path.ShouldEqual("/health/live");
    [Fact] void should_check_startup_over_http() => _container.StartupProbe.HttpGet.Path.ShouldEqual("/health/startup");
    [Fact] void should_request_resources() => _container.Resources.Requests["cpu"].ToString().ShouldEqual("250m");
    [Fact] void should_limit_resources() => _container.Resources.Limits["memory"].ToString().ShouldEqual("1Gi");
    [Fact] void should_share_a_scratch_directory() => _pod.Volumes.Single(volume => volume.Name == "app-chronicle-trust").EmptyDir.ShouldNotBeNull();
    [Fact] void should_mount_the_existing_claim() => _pod.Volumes.Single(volume => volume.Name == "app-data").PersistentVolumeClaim.ClaimName.ShouldEqual("crm-data");
    [Fact] void should_mount_the_volumes_in_the_application() => _container.VolumeMounts.Single(mount => mount.Name == "app-chronicle-trust").ReadOnlyProperty.ShouldEqual(true);
    [Fact] void should_keep_the_environment_mounts() => _container.VolumeMounts.Any(mount => mount.Name == "environment").ShouldBeTrue();
    [Fact] void should_run_the_init_container() => _pod.InitContainers.Single().Name.ShouldEqual("import-trust");
    [Fact] void should_run_the_init_container_from_the_image_of_the_application() => _pod.InitContainers.Single().Image.ShouldEqual(_container.Image);
    [Fact] void should_give_the_init_container_its_environment() => _pod.InitContainers.Single().Env.Single().Value.ShouldEqual("copy");
    [Fact] void should_mount_a_volume_in_the_init_container() => _pod.InitContainers.Single().VolumeMounts.Single().Name.ShouldEqual("app-chronicle-trust");
    [Fact] void should_not_let_the_init_container_escalate() => _pod.InitContainers.Single().SecurityContext.AllowPrivilegeEscalation.ShouldEqual(false);
}
#endif
