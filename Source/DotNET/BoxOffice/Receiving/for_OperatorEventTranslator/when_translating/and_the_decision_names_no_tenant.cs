// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving.for_OperatorEventTranslator.given;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Receiving.for_OperatorEventTranslator.when_translating;

public class and_the_decision_names_no_tenant : an_operator_event_translator
{
    async Task Because()
    {
        await _translator.Translate(new TenantReinstated(TenantId.NotSet), OrganizationContext(), Operator);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing() => _recorded.ShouldBeEmpty();
    [Fact] void should_log_that_it_ignored_it() => _logger.Messages.ShouldContain(_ => _.Level == LogLevel.Warning && _.Name == nameof(OperatorEventTranslatorLogging.NoTenant));
}
#endif
