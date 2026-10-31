// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Seats.ReleaseSeat;

/// <summary>
/// Takes a seat back from a person, so someone else can have it. Releasing a seat nobody holds records nothing.
/// </summary>
/// <param name="Holder">Who holds the seat.</param>
[Command]
[Authorize(Policy = BoxOfficePolicies.ManageSeats)]
public record ReleaseSeat(SeatHolder Holder) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => TenantStream.Id;

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="roster">Who holds the tenant's seats, read fresh from the event log.</param>
    /// <returns><see cref="SeatUnassigned"/>, or nothing when nobody holds the seat.</returns>
    public IEnumerable<object> Handle(SeatRoster? roster) =>
        roster?.Holdings.FirstOrDefault(_ => SeatHolder.Of(_.Holder) == Holder) is { } holding ? [new SeatUnassigned(holding.Holder)] : [];
}
