// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.given;

namespace Cratis.Showrunner.Notes.Collecting.Receiving.for_AppEventTranslator.when_translating;

/// <summary>
/// A tenant is its namespace's exact name: <c>FJORD-LOGISTICS</c> and <c>fjord-logistics</c> are two tenants.
/// </summary>
public class and_the_tenant_differs_only_in_case : an_app_event_translator
{
    async Task Because()
    {
        await _translator.Translate(Given(Tenant.Value.ToUpperInvariant()), InboxContext(), Application);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing() => _recorded.Count.ShouldEqual(0);
}
#endif
