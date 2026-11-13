// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Events;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Manifest = Cratis.Choreography.Availability;
using Names = Cratis.Choreography.Availability.Audiences;

namespace Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.given;

/// <summary>
/// A product that starts against an in-process event store, which keeps what earlier starts reported.
/// </summary>
public class a_product_reporting : Specification
{
    protected EventStoreForTesting _eventStore;
    protected IImmutableList<AppendedEvent> _reported;

    /// <summary>
    /// Gets a manifest that declares Plus and Preview, and a gate that opens for Plus.
    /// </summary>
    protected static Manifest.AvailabilityManifest PlusAndPreview => Declares(["Plus", "Preview"], "Billing", ["Plus"]);

    void Establish() => _eventStore = new();

    void Destroy() => _eventStore.Dispose();

    /// <summary>
    /// Describes what a model declares.
    /// </summary>
    /// <param name="audiences">The audiences declared, in this order.</param>
    /// <param name="gate">What the gate guards.</param>
    /// <param name="opensFor">The audiences that open the gate.</param>
    /// <returns>The manifest.</returns>
    protected static Manifest.AvailabilityManifest Declares(string[] audiences, string gate, string[] opensFor) =>
        new(
            "Ledger/availability.screenplay",
            [.. audiences.Select(audience => new Manifest.AudienceDeclaration(new Names.AudienceName(audience), $"{audience} customers", string.Empty))],
            [],
            [new Manifest.AvailabilityGateEntry(gate, [new Manifest.AvailabilityAlternative([.. opensFor.Select(audience => new Names.AudienceName(audience))], [])])],
            [],
            []);

    /// <summary>
    /// Creates the reporter of a manifest, as one start of the product would.
    /// </summary>
    /// <param name="manifest">What the model declares.</param>
    /// <param name="version">The version of the build.</param>
    /// <param name="environment">The environment reported from.</param>
    /// <returns>The reporter.</returns>
    protected static AvailabilityReporter ReporterFor(Manifest.AvailabilityManifest manifest, string version = "1.0.0", string environment = "Staging") =>
        new(
            new Manifest.InMemoryAvailabilityManifest(manifest),
            Options.Create(new GuestListOptions { Product = "ledger", Environment = environment, Version = version }),
            new Manifest.AvailabilityOptions(),
            NullLogger<AvailabilityReporter>.Instance);

    /// <summary>
    /// Reads everything the product reported.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    protected async Task ReadReported() =>
        _reported = await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);

    /// <summary>
    /// Gets a declaration the product reported.
    /// </summary>
    /// <param name="index">Which one, in the order they were reported.</param>
    /// <returns>The declaration.</returns>
    protected AvailabilityDeclared Declared(int index = 0) => (AvailabilityDeclared)_reported[index].Content;
}
#endif
