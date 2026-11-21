// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_DesiredPerformanceBuilder;

public class when_building_ante_with_branding_assets : Specification
{
    DesiredPerformance _desired;
    V1PodSpec _ante;

    void Because()
    {
        _desired = DesiredPerformanceBuilder.Build(ProxySamples.ConfiguredApplication(), new("chronicle-connection", "connectionString"), PerformanceSamples.Environment(), PerformanceSamples.Keys());
        _ante = _desired.Deployments.Single(deployment => deployment.Metadata.Name == "crm-ante").Spec.Template.Spec;
    }

    [Fact] void should_mount_the_first_file_on_its_own() => _ante.Containers[0].VolumeMounts.Single(mount => mount.MountPath == "/app/wwwroot/brand/logo.svg").SubPath.ShouldEqual("logo.svg");
    [Fact] void should_mount_the_second_file_on_its_own() => _ante.Containers[0].VolumeMounts.Single(mount => mount.MountPath == "/app/wwwroot/brand/lobby.css").SubPath.ShouldEqual("lobby.css");
    [Fact] void should_not_mount_the_folder_as_a_whole() => _ante.Containers[0].VolumeMounts.Any(mount => mount.MountPath == "/app/wwwroot/brand").ShouldBeFalse();
    [Fact] void should_read_the_files_from_their_config_map() => _ante.Volumes.Single(volume => volume.Name == "brand").ConfigMap.Name.ShouldEqual("crm-brand");
    [Fact] void should_keep_the_legal_terms_mounted_as_a_folder() => _ante.Containers[0].VolumeMounts.Single(mount => mount.MountPath == AnteContract.LegalDirectory).SubPath.ShouldBeNull();
    [Fact] void should_not_mount_a_key_ring_on_authproxy_that_was_not_asked_for() => _desired.Deployments.Single(deployment => deployment.Metadata.Name == "crm-authproxy").Spec.Template.Spec.Volumes.Any(volume => volume.Name == "keys").ShouldBeFalse();
}
#endif
