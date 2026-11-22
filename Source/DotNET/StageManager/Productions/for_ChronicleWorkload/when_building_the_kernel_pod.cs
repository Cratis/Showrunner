// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;
using k8s.Models;

namespace Cratis.Showrunner.StageManager.Productions.for_ChronicleWorkload;

public class when_building_the_kernel_pod : Specification
{
    V1StatefulSet _statefulSet;

    void Because()
    {
        var production = ProductionSamples.Production(ProductionSamples.External());
        _statefulSet = ChronicleWorkload.StatefulSet(production, ChronicleKernelConfiguration.Build(production.Spec, ProductionSamples.Names));
    }

    [Fact] void should_make_the_mounted_connection_files_readable_by_the_kernel() => _statefulSet.Spec.Template.Spec.SecurityContext.FsGroup.ShouldEqual(ChronicleWorkload.FileGroup);
}
#endif
