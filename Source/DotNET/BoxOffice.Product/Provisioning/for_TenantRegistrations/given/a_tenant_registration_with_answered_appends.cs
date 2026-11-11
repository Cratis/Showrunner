// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;

/// <summary>
/// A tenant registration whose once-only appends can be answered with a given result, for the tenant's own log or the outbox.
/// </summary>
public class a_tenant_registration_with_answered_appends : a_tenant_registration
{
    /// <summary>
    /// Creates the registrations over an event store.
    /// </summary>
    /// <param name="store">The event store the registrations record and report with.</param>
    /// <returns>The registrations.</returns>
    protected TenantRegistrations RegistrationsOver(IEventStore store)
    {
        var client = Substitute.For<IChronicleClient>();
        client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(store);
        return new(client, _provisioner, Microsoft.Extensions.Logging.Abstractions.NullLogger<TenantRegistrations>.Instance, _closing);
    }

    /// <summary>
    /// Creates the registrations over a store whose tenant log answers once-only appends with a result.
    /// </summary>
    /// <param name="answer">The result.</param>
    /// <returns>The registrations.</returns>
    protected TenantRegistrations RegistrationsWhoseTenantLogAnswers(AppendResult answer) => RegistrationsOver(AppendAnswers.Answering(_eventStore, answer));

    /// <summary>
    /// Creates the registrations over a store whose outbox answers once-only appends with a result.
    /// </summary>
    /// <param name="answer">The result.</param>
    /// <returns>The registrations.</returns>
    protected TenantRegistrations RegistrationsWhoseOutboxAnswers(AppendResult answer) => RegistrationsOver(AppendAnswers.AnsweringForTheOutbox(_eventStore, answer));
}
#endif
