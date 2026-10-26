// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing.for_GrantLapsing.given;
using Cratis.Showrunner.Usher.Contracts.Access;

namespace Cratis.Showrunner.Usher.Access.Lapsing.for_GrantLapsing.when_lapsing_due_grants;

public class and_one_grant_has_passed_its_end : grant_lapsing
{
    static readonly ValidUntil _passed = new(Now.AddMinutes(-1));
    static readonly ValidUntil _ahead = new(Now.AddHours(1));
    IssueId _otherIssueId;
    IEventStore _namespace;
    bool _otherLapsed;

    void Establish()
    {
        _otherIssueId = IssueId.New();

        // The in-process store folds events but cannot list read-model instances; serve the grants it would hold.
        _namespace = Substitute.For<IEventStore>();
        _namespace.EventLog.Returns(_eventStore.EventLog);
        _namespace.EventTypes.Returns(_eventStore.EventTypes);
        _namespace.ReadModels.GetInstances<GrantToLapse>().Returns([new GrantToLapse(_issueId, _passed), new GrantToLapse(_otherIssueId, _ahead)]);
    }

    async Task Because()
    {
        await HappenedOn(_issueId, new ReporterGrantedAccess(_passed));
        await HappenedOn(_otherIssueId, new ReporterGrantedAccess(_ahead));
        await _lapsing.LapseDueGrants(_namespace);
        await ReadEventsOf(_otherIssueId);
        _otherLapsed = _events.Any(_ => _.Content is AccessGrantLapsed);
        await ReadEventsOf(_issueId);
    }

    [Fact] void should_lapse_the_grant_that_passed_its_end() => _events[^1].Content.ShouldEqual(new AccessGrantLapsed(_passed));
    [Fact] void should_not_lapse_the_grant_still_ahead() => _otherLapsed.ShouldBeFalse();
}
#endif
