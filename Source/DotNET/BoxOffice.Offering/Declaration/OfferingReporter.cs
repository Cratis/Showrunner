// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// Reports the product's offering to the operator as <see cref="OfferingDeclared"/>, once per change.
/// </summary>
/// <remarks>
/// <para>
/// The declaration goes to the outbox of the namespace given, under the product's id, because the operator reads
/// what a product says about itself from there. What was last reported is read from the outbox itself, so the answer
/// survives restarts and does not depend on any read model catching up.
/// </para>
/// <para>
/// Several instances of a product start together. The append carries an idempotency key made from the declaration and
/// the declaration it replaces, so instances that all find the same last declaration append the same key, and the
/// event store keeps one. Declaring A, then B, then A again is three reports, because each replaces a different last
/// declaration.
/// </para>
/// </remarks>
/// <param name="offering">The product's offering.</param>
/// <param name="logger">The logger.</param>
public class OfferingReporter(ProductOffering offering, ILogger<OfferingReporter> logger)
{
    /// <summary>
    /// Reports the offering to the outbox of an event store unless it already reports exactly this.
    /// </summary>
    /// <param name="eventStore">The event store, in the namespace the operator reads the product's outbox from.</param>
    /// <returns>Whether anything was reported.</returns>
    /// <exception cref="OfferingNotReported">Thrown when the event store refuses the declaration.</exception>
    public async Task<OfferingReport> Report(IEventStore eventStore)
    {
        var outbox = eventStore.GetEventSequence(EventSequenceId.Outbox);
        var declared = eventStore.EventTypes.GetEventTypeFor(typeof(OfferingDeclared));
        var reported = await outbox.GetForEventSourceIdAndEventTypes(offering.Product, [declared]);
        var last = reported.Count > 0 ? reported[^1] : null;
        var declaration = offering.ToDeclaration();

        if (last?.Content is OfferingDeclared previous && OfferingDeclarations.AreSame(previous, declaration))
        {
            logger.OfferingUnchanged(offering.Product);
            return OfferingReport.Unchanged;
        }

        var key = new IdempotencyKey($"boxoffice-offering-{OfferingDeclarations.Fingerprint(declaration)}-{last?.Context.SequenceNumber.Value.ToString() ?? "first"}");
        var result = await outbox.AppendOnce(offering.Product, declaration, key);
        if (result.HasIdempotencyConflicts)
        {
            // Another instance reported the same declaration over the same last one.
            logger.OfferingUnchanged(offering.Product);
            return OfferingReport.Unchanged;
        }

        if (!result.IsSuccess)
        {
            throw new OfferingNotReported(offering.Product, result);
        }

        logger.OfferingReported(offering.Product, declaration.Modules.Count(), declaration.Meters.Count());
        return OfferingReport.Reported;
    }
}
