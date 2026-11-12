// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Holding;

/// <summary>
/// The audiences held at one level, in one stream: a tenant's, a user's of that tenant, or the system's. The tenant's and the
/// users' live in the tenant's own namespace, the system's in the product's own.
/// </summary>
/// <remarks>
/// <para>
/// It is a self-reducing read model. It keeps the newest decision per audience, a withdrawal included, so a decision that
/// arrives late, such as a grant older than the withdrawal already recorded, does not bring an audience back. Decisions are
/// compared by when the operator took them; when two carry the same time, the one recorded last wins. A decision at one
/// level never touches another level, because each level has its own stream.
/// </para>
/// <para>
/// A grant for an audience the model does not declare is kept like any other: it never matches a gate, and is reported by
/// the <c>UndeclaredGrant</c> query. Declaring the audience later makes it live without a replay.
/// </para>
/// </remarks>
/// <param name="Id">The stream: see <see cref="GuestListStreams"/>.</param>
/// <param name="Level">The level the audiences are held at.</param>
/// <param name="Decisions">The newest decision per audience.</param>
public record AudienceHoldings(string Id, AudienceLevel Level, IEnumerable<AudienceDecision> Decisions)
{
    /// <summary>
    /// Gets the audiences granted, in the order of their names.
    /// </summary>
    /// <returns>The audiences.</returns>
    public IReadOnlyList<AudienceName> HeldAudiences() =>
        [.. Decisions.Where(decision => decision.Granted).Select(decision => decision.Audience).OrderBy(audience => audience.Value, StringComparer.Ordinal)];

    /// <summary>
    /// Records a grant, unless a newer decision about the audience is already recorded.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static AudienceHoldings Reduce(AudienceGrantRecorded @event, AudienceHoldings? current, EventContext context) =>
        Decide(@event.Level, @event.Audience, true, current, context);

    /// <summary>
    /// Records a withdrawal, unless a newer decision about the audience is already recorded.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static AudienceHoldings Reduce(AudienceWithdrawalRecorded @event, AudienceHoldings? current, EventContext context) =>
        Decide(@event.Level, @event.Audience, false, current, context);

    static AudienceHoldings Decide(AudienceLevel level, AudienceName audience, bool granted, AudienceHoldings? current, EventContext context)
    {
        var state = current ?? new(context.EventSourceId.Value, level, []);
        var existing = state.Decisions.FirstOrDefault(decision => decision.Audience == audience);
        if (existing is not null && existing.Decided > context.Occurred)
        {
            return state;
        }

        var decision = new AudienceDecision(audience, granted, context.Occurred);
        return state with { Decisions = [.. state.Decisions.Where(held => held.Audience != audience), decision] };
    }
}
