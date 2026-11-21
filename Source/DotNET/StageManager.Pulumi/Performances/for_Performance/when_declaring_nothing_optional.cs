// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorPerformanceSpec = Cratis.Showrunner.StageManager.Performances.PerformanceSpec;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances.for_Performance;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_nothing_optional : Specification
{
    DeclaredResources _declared;
    OperatorPerformanceSpec _spec;

    async Task Because()
    {
        _declared = await DeclaredResources.Run(() => _ = new Performance(
            "worker",
            new PerformanceArgs
            {
                Namespace = PulumiSpecs.Namespace,
                ProductionName = "keldakraft",
                Application = new ApplicationArgs { Image = "ghcr.io/kelda/worker:1.2.3" },
                Environment = new PerformanceEnvironmentArgs { Name = "production", ConfigMapName = "keldakraft-environment" },
            }));
        _spec = _declared.SpecOf<OperatorPerformanceSpec>("Performance");
    }

    [Fact] void should_run_no_authproxy() => _spec.AuthProxy.Enabled.ShouldBeFalse();
    [Fact] void should_run_no_ante() => _spec.Onboarding.Enabled.ShouldBeFalse();
    [Fact] void should_create_no_ingress() => _spec.Ingress.Enabled.ShouldBeFalse();
    [Fact] void should_write_no_secret() => _declared.Secrets.ShouldBeEmpty();
    [Fact] void should_write_no_config_map() => _declared.ConfigMaps.ShouldBeEmpty();
}
#endif
