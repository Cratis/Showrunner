// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.given;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.when_publishing;

public class to_a_desk_with_an_audience : an_outbox_publisher
{
    NamedTag[] _tags;

    async Task Because()
    {
        _publisher = new(_client, _logger, Options.Create(new UsherOptions { DeskAudience = "direct" }));
        var context = ContextFor();
        await _publisher.IssueDescribed(IssueEvents.Described(Reporters.Identity), context, DeliveryOf(context));
        await ReadOutbox();
        _tags = [.. _outbox.Single().Context.NamedTags];
    }

    [Fact] void should_address_the_fact_to_the_desk() => _tags.ShouldContain(new NamedTag(WellKnownTags.Audience, "direct"));
}
#endif
