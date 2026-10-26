// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.given;

namespace Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.when_translating;

public class and_the_issue_was_reported_in_the_tenant : a_desk_event_translator
{
    async Task Because()
    {
        await Reported();
        await _translator.Translate(new IssueResolved(Tenant.Value, "Fixed in 4.12.3."), InboxContext(), Desk);
        await ReadRecorded();
    }

    [Fact] void should_record_the_support_fact_after_the_report() => _recorded.Count.ShouldEqual(2);
    [Fact] void should_record_support_resolving_the_issue() => _recorded[^1].Content.ShouldEqual(new SupportResolvedIssue("Fixed in 4.12.3."));
    [Fact] void should_record_it_for_the_issue() => _recorded[^1].Context.EventSourceId.Value.ShouldEqual(_issueId.Value.ToString());
    [Fact] void should_keep_when_the_desk_recorded_it() => _recorded[^1].Context.Occurred.ShouldEqual(Occurred);
    [Fact] void should_keep_the_correlation_id() => _recorded[^1].Context.CorrelationId.ShouldEqual(_correlationId);
}
#endif
