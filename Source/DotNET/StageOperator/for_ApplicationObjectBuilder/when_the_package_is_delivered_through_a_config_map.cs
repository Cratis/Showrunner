// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;

namespace Cratis.Showrunner.StageOperator.for_ApplicationObjectBuilder;

public class when_the_package_is_delivered_through_a_config_map : Specification
{
    DesiredObjects _result = null!;
    DesiredObjects _baked = null!;

    void Because()
    {
        var application = Applications.Create(package: "orders-package-1-1-0");
        _result = ApplicationObjectBuilder.Build(application, Applications.NewImage, 1, true, new StageOperatorOptions(), "orders-package-1-1-0");
        _baked = ApplicationObjectBuilder.Build(Applications.Create(), Applications.NewImage, 1, true, new StageOperatorOptions());
    }

    [Fact] void should_mount_only_the_archive_key_of_the_config_map() => Assert.Equal(("orders-package-1-1-0", "package.tar.gz"), (_result.Deployment.Spec.Template.Spec.Volumes.Single().ConfigMap.Name, _result.Deployment.Spec.Template.Spec.Volumes.Single().ConfigMap.Items.Single().Key));
    [Fact] void should_mount_it_read_only_in_stage() => Assert.Equal(("/stage/package-archive", true), (_result.Deployment.Spec.Template.Spec.Containers[0].VolumeMounts.Single().MountPath, _result.Deployment.Spec.Template.Spec.Containers[0].VolumeMounts.Single().ReadOnlyProperty));
    [Fact] void should_tell_stage_where_the_archive_is() => Assert.Contains(_result.Deployment.Spec.Template.Spec.Containers[0].Env, e => e.Name == "Stage__Published__PackagePath" && e.Value == "/stage/package-archive/package.tar.gz");
    [Fact] void should_not_mount_it_in_the_proxy() => Assert.Null(_result.Deployment.Spec.Template.Spec.Containers[1].VolumeMounts);
    [Fact] void should_mount_nothing_when_the_image_carries_the_package() => Assert.Null(_baked.Deployment.Spec.Template.Spec.Volumes);
    [Fact] void should_not_set_a_package_path_when_the_image_carries_the_package() => Assert.DoesNotContain(_baked.Deployment.Spec.Template.Spec.Containers[0].Env, e => e.Name == "Stage__Published__PackagePath");
}
#endif
