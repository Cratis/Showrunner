// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances.for_Performance;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_an_application_without_a_chronicle : Specification
{
    Exception _exception;

    async Task Because() => _exception = await Catch.Exception(async () => await DeclaredResources.Run(() => _ = new Performance(
        "keldakraft",
        new PerformanceArgs
        {
            Namespace = PulumiSpecs.Namespace,
            Application = new ApplicationArgs { Image = "ghcr.io/kelda/keldakraft:1.2.3" },
            Environment = new PerformanceEnvironmentArgs { Name = "production", ConfigMapName = "keldakraft-environment" },
        })));

    [Fact] void should_be_refused() => _exception.ShouldNotBeNull();
    [Fact] void should_say_which_inputs_name_the_chronicle() => _exception.ToString().ShouldContain("ProductionName or ChronicleConnectionSecret");
}
#endif
