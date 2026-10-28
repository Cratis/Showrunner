// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Receiving.for_DeskEventTranslator.when_translating;

public class and_the_event_names_no_tenant : a_desk_event_translator
{
    async Task Because()
    {
        await Reported();
        await _translator.Translate(new IssueAccepted(TenantId.NotSet), InboxContext(), Desk);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing_new() => _recorded.Count.ShouldEqual(1);
    [Fact] void should_log_that_it_ignored_it() => _logger.Messages.ShouldContain(_ => _.Level == LogLevel.Warning && _.Name == nameof(DeskEventTranslatorLogging.NoTenant));
}
#endif
