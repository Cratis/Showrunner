// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Holding;

/// <summary>
/// The parts suspended at one level, in one stream: a tenant's, a user's of that tenant, or the system's. The tenant's and
/// the users' live in the tenant's own namespace, the system's in the product's own. The streams are the ones the audiences
/// of the level are held in, see <see cref="GuestListStreams"/>.
/// </summary>
/// <remarks>
/// It is a self-reducing read model. It keeps the newest decision per part, a resume included, so a decision that arrives
/// late, such as a suspension older than the resume already recorded, does not suspend a part again. Decisions are compared
/// by when the operator took them; when two carry the same time, the one recorded last wins. A decision at one level never
/// touches another level, because each level has its own stream.
/// </remarks>
/// <param name="Id">The stream: see <see cref="GuestListStreams"/>.</param>
/// <param name="Level">The level the parts are suspended at.</param>
/// <param name="Decisions">The newest decision per part.</param>
public record SuspensionHoldings(string Id, SuspensionLevel Level, IEnumerable<PartDecision> Decisions)
{
    /// <summary>
    /// Records a suspension, unless a newer decision about the part is already recorded.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static SuspensionHoldings Reduce(PartSuspensionRecorded @event, SuspensionHoldings? current, EventContext context) =>
        Decide(@event.Level, @event.Part, true, current, context);

    /// <summary>
    /// Records a resume, unless a newer decision about the part is already recorded.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static SuspensionHoldings Reduce(PartResumeRecorded @event, SuspensionHoldings? current, EventContext context) =>
        Decide(@event.Level, @event.Part, false, current, context);

    static SuspensionHoldings Decide(SuspensionLevel level, PartPath part, bool suspended, SuspensionHoldings? current, EventContext context)
    {
        var state = current ?? new(context.EventSourceId.Value, level, []);
        var existing = state.Decisions.FirstOrDefault(decision => decision.Part == part);
        if (existing is not null && existing.Decided > context.Occurred)
        {
            return state;
        }

        var decision = new PartDecision(part, suspended, context.Occurred);
        return state with { Decisions = [.. state.Decisions.Where(held => held.Part != part), decision] };
    }
}
