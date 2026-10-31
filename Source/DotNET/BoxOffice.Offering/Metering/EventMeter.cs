// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// A meter that measures one kind of event with a function.
/// </summary>
/// <typeparam name="TEvent">The type of event to read.</typeparam>
/// <param name="id">The key of the declared meter this measures.</param>
/// <param name="measure">Measures an event; returns <see langword="null"/> when it used none of the meter.</param>
public class EventMeter<TEvent>(MeterId id, Func<TEvent, EventContext, MeterReading?> measure) : IMeter
    where TEvent : class
{
    /// <inheritdoc/>
    public MeterId Id { get; } = id;

    /// <inheritdoc/>
    public IReadOnlyCollection<Type> EventTypes { get; } = [typeof(TEvent)];

    /// <inheritdoc/>
    public MeterReading? Measure(object @event, EventContext context) => measure((TEvent)@event, context);
}
