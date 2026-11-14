// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Execution;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.given;

/// <summary>
/// What a product reports to the operator, in the order it happens, one minute apart.
/// </summary>
/// <typeparam name="TReadModel">The read model built from the reports.</typeparam>
public class a_product_that_reports<TReadModel> : Specification
    where TReadModel : class
{
    /// <summary>
    /// The environment the product reports from.
    /// </summary>
    protected static readonly EnvironmentName Production = "Production";

    /// <summary>
    /// The other environment the product reports from.
    /// </summary>
    protected static readonly EnvironmentName Staging = "Staging";

    protected ReadModelScenario<TReadModel> _scenario;
    ulong _sequenceNumber;

    /// <summary>
    /// Gets what is built at the end.
    /// </summary>
    protected TReadModel Built => _scenario.Instance!;

    /// <summary>
    /// Gets when the minute-th event happens.
    /// </summary>
    /// <param name="minute">The minute.</param>
    /// <returns>The time.</returns>
    protected static DateTimeOffset At(int minute) => new DateTimeOffset(2026, 11, 2, 9, 0, 0, TimeSpan.Zero).AddMinutes(minute);

    /// <summary>
    /// Declares audiences, and nothing else.
    /// </summary>
    /// <param name="environment">The environment reported from.</param>
    /// <param name="version">The version of the build.</param>
    /// <param name="audiences">The audiences.</param>
    /// <returns>The declaration.</returns>
    protected static AvailabilityDeclared Declares(EnvironmentName environment, ModelVersion version, params AudienceName[] audiences) =>
        new(
            Guests.Product,
            environment,
            version,
            "Ledger/availability.screenplay",
            false,
            [.. audiences.Select(audience => new AudienceDeclaration(audience, string.Empty, string.Empty))],
            [],
            [],
            [],
            []);

    /// <summary>
    /// Gets the receipt of a decision.
    /// </summary>
    /// <param name="environment">The environment that received it.</param>
    /// <param name="decidedAt">When the operator decided.</param>
    /// <returns>The receipt.</returns>
    protected static AudienceDecisionReceived Received(EnvironmentName environment, DateTimeOffset decidedAt) =>
        new(Guests.Product, environment, Contracts.Availability.AudienceDecision.Granted, AudienceLevel.Tenant, Guests.Acme, UserId.NotSet, Guests.Plus, decidedAt, SystemClientId.NotSet);

    void Establish() => _scenario = new();

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Lets reports reach the operator, one minute apart, after any that reached it before.
    /// </summary>
    /// <param name="events">The events, in the order they happen.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Happened(params object[] events) =>
        _scenario.ProcessAppendedEvents([.. events.Select(@event =>
        {
            var sequenceNumber = _sequenceNumber++;
            var context = EventContext.From(
                EventStoreName.NotSet,
                EventStoreNamespaceName.NotSet,
                EventType.Unknown,
                EventSourceType.Default,
                new EventSourceId(Guests.Product.Value),
                EventStreamType.All,
                EventStreamId.Default,
                sequenceNumber,
                CorrelationId.New(),
                At((int)sequenceNumber));
            return new AppendedEvent(context, @event);
        })]);
}
#endif
