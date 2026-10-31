// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Entitlements;

namespace Cratis.Showrunner.BoxOffice.Seats.AssignSeat;

/// <summary>
/// Gives a person one of the tenant's seats. Who holds a seat is the product's decision; how many seats there are is the
/// operator's. Assigning a seat to someone who already holds one records nothing.
/// </summary>
/// <param name="Holder">The person to give a seat.</param>
[Command]
[Authorize(Policy = BoxOfficePolicies.ManageSeats)]
public record AssignSeat(Person Holder) : ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => TenantStream.Id;

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="roster">Who holds the tenant's seats, read fresh from the event log; another assignment or release
    /// recorded meanwhile refuses this one instead of overfilling the pool.</param>
    /// <returns><see cref="SeatAssigned"/>, or nothing when the person holds a seat.</returns>
    public IEnumerable<object> Handle(SeatRoster? roster) =>
        roster?.Holds(SeatHolder.Of(Holder)) == true ? [] : [new SeatAssigned(Holder)];
}

/// <summary>
/// Validates <see cref="AssignSeat"/>.
/// </summary>
public class AssignSeatValidator : CommandValidator<AssignSeat>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssignSeatValidator"/> class.
    /// </summary>
    /// <param name="roster">Who holds the tenant's seats.</param>
    /// <param name="capacity">How many seats the tenant may assign.</param>
    /// <param name="clock">The <see cref="TimeProvider"/> deciding whether the seats have ended.</param>
    public AssignSeatValidator(SeatRoster? roster, SeatCapacity? capacity, TimeProvider clock)
    {
        RuleFor(c => c.Holder.IdentityProviderSubject)
            .Must(subject => !string.IsNullOrWhiteSpace(subject))
            .WithMessage("Choose who gets the seat.");
        RuleFor(c => c.Holder)
            .Must(holder => roster?.Holds(SeatHolder.Of(holder)) == true || (roster?.Held ?? 0) < (capacity?.On(DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime)) ?? 0))
            .WithMessage("There are no free seats. Release a seat or ask for more.");
    }
}
