// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_SpecValidator;

public class when_validating_published_edges : Specification
{
    Performance _performance;
    IReadOnlyList<string> _problems;

    void Establish()
    {
        _performance = PerformanceSamples.PublishedPerformance();
        _performance.Spec.Application.Published!.TrustedProxies = ["10.0.0.1/33", "::1/129", "host", "10.0.0.1/x"];
        _performance.Spec.Application.Published.Notes!.CollectorUrl = "ftp://collector.example.invalid";
        _performance.Spec.Application.Published.Notes.PublicationId = " ";
        _performance.Spec.AuthProxy.Providers[0].Identity = " ";
    }

    void Because() => _problems = SpecValidator.Validate(_performance);

    [Fact]
    void should_keep_cidr_url_notes_and_blank_provider_messages_in_order() => _problems.ShouldEqual<string>(new[]
    {
        "The provider identity ' ' may only hold letters, digits, spaces, hyphens and underscores.",
        "The trusted proxy '10.0.0.1/33' is neither an IP address nor a CIDR network.",
        "The trusted proxy '::1/129' is neither an IP address nor a CIDR network.",
        "The trusted proxy 'host' is neither an IP address nor a CIDR network.",
        "The trusted proxy '10.0.0.1/x' is neither an IP address nor a CIDR network.",
        "application.published.notes.collectorUrl must be an absolute http or https URL.",
        "application.published.notes.publicationId is required."
    });
}
#endif
