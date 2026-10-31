// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_OfferingReporter.given;

/// <summary>
/// A product that starts against an in-process event store, which keeps what earlier starts reported.
/// </summary>
public class a_product_starting : Specification
{
    protected EventStoreForTesting _eventStore;
    protected ProductId _product;
    protected IImmutableList<AppendedEvent> _reported;

    void Establish()
    {
        _eventStore = new();
        _product = ProductId.New();
    }

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Creates the reporter of an offering of the product, as one start of it would.
    /// </summary>
    /// <param name="declare">Declares the modules and meters.</param>
    /// <returns>The reporter.</returns>
    protected OfferingReporter ReporterFor(Action<ProductOfferingBuilder> declare)
    {
        var builder = new ProductOfferingBuilder().Product(_product);
        declare(builder);
        return new(builder.Build(), Microsoft.Extensions.Logging.Abstractions.NullLogger<OfferingReporter>.Instance);
    }

    /// <summary>
    /// The declaration of a product with a core module and an analytics module that needs it.
    /// </summary>
    /// <param name="builder">The builder.</param>
    protected static void CoreAndAnalytics(ProductOfferingBuilder builder) => builder
        .Module("core", "Core", module => module.Availability(ModuleAvailability.Included))
        .Module("analytics", "Analytics", module => module.Requires("core"))
        .Meter("compute", "Compute", "minutes");

    /// <summary>
    /// Reads everything the product reported.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadReported() =>
        _reported = await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);
}
#endif
