// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.given;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;

namespace Cratis.Showrunner.Usher.Publishing.for_OutboxPublisher.when_publishing;

public class a_described_issue : an_outbox_publisher
{
    ReporterDescribedIssue _described;
    IssueReported _published;
    bool _eventLogHasEvents;

    async Task Because()
    {
        _described = IssueEvents.Described(Reporters.Identity);
        var context = ContextFor();
        await _publisher.IssueDescribed(_described, context, DeliveryOf(context));
        await ReadOutbox();
        _published = _outbox.Single().Content as IssueReported;
        _eventLogHasEvents = await _eventStore.GetEventSequence(EventSequenceId.Log).HasEventsFor(_issueId);
    }

    [Fact] void should_publish_one_event() => _outbox.Count.ShouldEqual(1);
    [Fact] void should_publish_it_as_issue_reported() => _published.ShouldNotBeNull();
    [Fact] void should_publish_it_for_the_issue() => _outbox.Single().Context.EventSourceId.Value.ShouldEqual(_issueId.Value.ToString());
    [Fact] void should_name_the_namespace_as_tenant() => _published.TenantId.ShouldEqual(new TenantId(Tenant.Value));
    [Fact] void should_carry_the_location() => _published.Location.ShouldEqual(_described.Location);
    [Fact] void should_carry_the_summary() => _published.Summary.ShouldEqual(_described.Summary);
    [Fact] void should_carry_the_description() => _published.Description.ShouldEqual(_described.Description);
    [Fact] void should_carry_the_application_name() => _published.ApplicationName.ShouldEqual(_described.ApplicationName);
    [Fact] void should_carry_the_application_version() => _published.ApplicationVersion.ShouldEqual(_described.ApplicationVersion);
    [Fact] void should_not_carry_the_reporter_subject() => JsonSerializer.Serialize(_published).ShouldNotContain(Reporters.Subject.Value);
    [Fact] void should_keep_when_it_happened() => _outbox.Single().Context.Occurred.ShouldEqual(Occurred);
    [Fact] void should_keep_the_correlation_id() => _outbox.Single().Context.CorrelationId.ShouldEqual(_correlationId);
    [Fact] void should_not_append_anything_to_the_event_log() => _eventLogHasEvents.ShouldBeFalse();
}
#endif
