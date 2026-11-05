// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.BoxOffice.Product.Tenancy;
using Cratis.Showrunner.BoxOffice.Receiving;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Product.Activity;

/// <summary>
/// Tells the operator a person was active, at most once per person, tenant and day.
/// </summary>
/// <remarks>
/// Two things keep a day's repeats from the event log. The reporter remembers the people it reported today, so a repeat
/// costs nothing. That memory is lost when the product restarts, so the report is also appended once by a key of the
/// person, tenant and day, which the event log keeps: a repeat after a restart, or on another replica, appends nothing.
/// </remarks>
/// <param name="stores">The <see cref="ITenantStores"/> that finds the tenant.</param>
/// <param name="time">The <see cref="TimeProvider"/> the day is counted by.</param>
/// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
public class ActivityReporter(ITenantStores stores, TimeProvider time, ILogger<ActivityReporter> logger) : IReportActivity
{
    readonly ConcurrentDictionary<string, bool> _reportedToday = new();
    readonly Lock _lock = new();
    ActivityDay _today = new(DateOnly.MinValue);

    /// <summary>
    /// Gets the key a person's activity on a day is recorded with.
    /// </summary>
    /// <param name="tenant">The tenant the person was active in.</param>
    /// <param name="person">The person who was active.</param>
    /// <param name="day">The day the person was active.</param>
    /// <returns>The <see cref="IdempotencyKey"/>.</returns>
    public static IdempotencyKey KeyFor(TenantId tenant, Person person, ActivityDay day)
    {
        var identity = string.Join('\n', tenant.Value, person.IdentityProvider.Value, person.IdentityProviderSubject.Value, day.Value.ToString("O"));
        return new($"activity-{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))}");
    }

    /// <inheritdoc/>
    public async Task<ActivityOutcome> Report(TenantId tenant, Person person, CancellationToken cancellationToken = default)
    {
        var day = new ActivityDay(DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime));
        var remembered = RememberedFor(tenant, person, day);
        if (_reportedToday.ContainsKey(remembered))
        {
            return ActivityOutcome.AlreadyReported;
        }

        if (await stores.For(tenant, cancellationToken) is not { } store)
        {
            logger.UnknownTenant(tenant);
            return ActivityOutcome.UnknownTenant;
        }

        // The organization is answered from the event log, not from a read model: a read model can be behind the facts.
        var identified = await store.EventLog.GetForEventSourceIdAndEventTypes(
            TenantStream.Id,
            [store.EventTypes.GetEventTypeFor(typeof(TenantOrganizationIdentified))]);
        if (identified.Count == 0 || identified[^1].Content is not TenantOrganizationIdentified { Organization: var organization })
        {
            logger.OrganizationNotKnown(tenant);
            return ActivityOutcome.OrganizationNotKnown;
        }

        var result = await store.GetEventSequence(EventSequenceId.Outbox).AppendOnce(
            organization,
            new SeatHolderActive(tenant, person, day),
            KeyFor(tenant, person, day));

        // A conflict is the same person and day reported with other details, such as a changed name: still reported.
        if (!result.IsSuccess && !result.WasAlreadyAppended && !result.HasIdempotencyConflicts)
        {
            throw new ActivityNotReported(tenant, result);
        }

        _reportedToday[remembered] = true;
        return result.WasAlreadyAppended || result.HasIdempotencyConflicts ? ActivityOutcome.AlreadyReported : ActivityOutcome.Reported;
    }

    string RememberedFor(TenantId tenant, Person person, ActivityDay day)
    {
        lock (_lock)
        {
            if (_today != day)
            {
                // A new day: nobody has been reported on it yet.
                _today = day;
                _reportedToday.Clear();
            }
        }

        return string.Join('\n', tenant.Value, person.IdentityProvider.Value, person.IdentityProviderSubject.Value);
    }
}
