// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.given;

namespace Cratis.Showrunner.Notes.Receiving.for_ReceiverEventTranslator.when_translating;

/// <summary>
/// A tenant is its namespace's exact name: <c>Fjord-Logistics</c> and <c>fjord-logistics</c> are two tenants.
/// </summary>
public class and_the_tenant_differs_only_in_case : a_receiver_event_translator
{
    async Task Because()
    {
        await Given();
        await _translator.Translate(new NoteAcknowledged(Tenant.Value.ToUpperInvariant()), InboxContext(), Collecting);
        await ReadRecorded();
    }

    [Fact] void should_record_nothing() => _recorded.Count(_ => _.Content is ReceiverAcknowledgedNote).ShouldEqual(0);
}
#endif
