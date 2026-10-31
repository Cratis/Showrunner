// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// Measures one declared meter of the product from facts the product already records. Box Office adds no second
/// event for it: the meter reads the product's own events, such as the end of a session, and says how much of the
/// meter each one used.
/// </summary>
/// <remarks>
/// A meter is a projection that has no state of its own. Box Office keeps the running totals, per tenant, meter and
/// day, beside the event log. Register meters with <c>AddMeter</c>.
/// </remarks>
public interface IMeter
{
    /// <summary>
    /// Gets the key of the declared meter this measures.
    /// </summary>
    MeterId Id { get; }

    /// <summary>
    /// Gets the types of the product's events this meter reads.
    /// </summary>
    IReadOnlyCollection<Type> EventTypes { get; }

    /// <summary>
    /// Measures an event.
    /// </summary>
    /// <param name="event">The event, of one of <see cref="EventTypes"/>.</param>
    /// <param name="context">The event's context.</param>
    /// <returns>The reading, or <see langword="null"/> when the event used none of the meter.</returns>
    MeterReading? Measure(object @event, EventContext context);
}
