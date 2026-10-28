// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.when_translating;

public class and_the_issue_was_never_reported : a_desk_event_translator
{
    async Task Because()
    {
        await _translator.Translate(new IssueTriaged(Tenant.Value), InboxContext(), Desk);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing() => _recorded.ShouldBeEmpty();
    [Fact] void should_log_that_it_ignored_it() => _logger.Messages.ShouldContain(_ => _.Level == LogLevel.Warning && _.Name == nameof(DeskEventTranslatorLogging.UnknownIssue));
}
#endif
