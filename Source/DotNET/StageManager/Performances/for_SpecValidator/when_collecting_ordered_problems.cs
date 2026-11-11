// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_SpecValidator;

public class when_collecting_ordered_problems : Specification
{
    Performance _performance;
    IReadOnlyList<string> _problems;

    void Establish()
    {
        _performance = PerformanceSamples.PublishedPerformance();
        var spec = _performance.Spec;
        spec.Chronicle = new() { ConnectionSecretRef = new() { Name = " " } };
        spec.ProductionRef = new() { Name = " " };
        spec.Application.Image = " ";
        spec.Application.Sinks = [" "];
        spec.Application.Published!.Version = " ";
        spec.AuthProxy.Providers[0].Identity = "bad/id";
        spec.AuthProxy.Providers.Add(new() { Identity = "GitHub" });
        spec.Environment.Name = " ";
        spec.Environment.ConfigMapRef.Name = " ";
        spec.Lifetime = new();
    }

    void Because() => _problems = SpecValidator.Validate(_performance);

    [Fact]
    void should_keep_every_message_in_validation_order() => _problems.ShouldEqual<string>(new[]
    {
        "application.image is required.",
        "environment.name is required.",
        "environment.configMapRef.name is required.",
        "application.sinks may not hold an empty name.",
        "application.sinks is for a traditional application: a published Stage package declares its sinks in its manifest.",
        "The provider identity 'bad/id' may only hold letters, digits, spaces, hyphens and underscores.",
        "The provider identity 'GitHub' is used twice.",
        "application.published.version is required.",
        "lifetime needs exactly one of permanent: true and until."
    });
}
#endif
