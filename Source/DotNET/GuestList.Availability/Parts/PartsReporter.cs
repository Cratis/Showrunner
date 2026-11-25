// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Arc.Availability;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.GuestList.Availability.Declaration;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using Cratis.Showrunner.GuestList.Contracts.Suspensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Availability.Parts;

/// <summary>
/// Reports to the operator every part of the application, so an operator can pick one to suspend.
/// </summary>
/// <remarks>
/// The parts are the ones the availability engine discovered: every module, feature, slice, command, query and screen,
/// declared in the model or not. The report is the whole list, appended to the outbox only when it differs from the last one
/// reported, so a restart that changes nothing reports nothing.
/// </remarks>
/// <param name="manifest">The <see cref="IAvailabilityManifest"/> the engine lists the parts in.</param>
/// <param name="options">GuestList's options, which name the product.</param>
/// <param name="availability">The options of the availability engine, which name the environment.</param>
/// <param name="logger">The logger.</param>
public class PartsReporter(IAvailabilityManifest manifest, IOptions<GuestListOptions> options, AvailabilityOptions availability, ILogger<PartsReporter> logger)
{
    /// <summary>
    /// Gets what the application would report now.
    /// </summary>
    /// <returns>The declaration.</returns>
    public PartsDeclared Declaration() =>
        new(
            options.Value.Product,
            Environment(),
            [
                .. manifest.Current.Parts
                    .Where(part => PartPath.IsValidPath(part.Path))
                    .DistinctBy(part => part.Path, StringComparer.Ordinal)
                    .OrderBy(part => part.Path, StringComparer.Ordinal)
                    .Select(part => new PartDeclaration(part.Kind, new PartPath(part.Path), part.Name ?? string.Empty))
            ]);

    /// <summary>
    /// Reports the parts, unless they are what was last reported.
    /// </summary>
    /// <param name="eventStore">The product's event store.</param>
    /// <returns>What reporting came to.</returns>
    /// <exception cref="AvailabilityNotReported">The report could not be appended.</exception>
    public async Task<AvailabilityReport> Report(IEventStore eventStore)
    {
        var outbox = eventStore.GetEventSequence(EventSequenceId.Outbox);
        var declared = eventStore.EventTypes.GetEventTypeFor(typeof(PartsDeclared));
        var reported = await outbox.GetForEventSourceIdAndEventTypes(options.Value.Product.Value, [declared]);
        var last = reported.Count > 0 ? reported[^1] : null;
        var declaration = Declaration();

        if (last is null && !declaration.Parts.Any())
        {
            return AvailabilityReport.NothingDeclared;
        }

        if (last?.Content is PartsDeclared previous && Canonical(previous) == Canonical(declaration))
        {
            logger.PartsUnchanged(options.Value.Product);
            return AvailabilityReport.Unchanged;
        }

        var key = new IdempotencyKey($"guestlist-parts-{Fingerprint(declaration)}-{last?.Context.SequenceNumber.Value.ToString() ?? "first"}");
        using var routing = string.IsNullOrWhiteSpace(options.Value.OperatorAudience) ? null : EventTags.Audience(options.Value.OperatorAudience);
        var result = await outbox.AppendOnce(options.Value.Product.Value, declaration, key);
        if (result.WasAlreadyAppended || result.HasIdempotencyConflicts)
        {
            logger.PartsUnchanged(options.Value.Product);
            return AvailabilityReport.Unchanged;
        }

        if (!result.IsSuccess)
        {
            throw new AvailabilityNotReported(options.Value.Product, result);
        }

        logger.PartsReported(options.Value.Product, declaration.Parts.Count());
        return AvailabilityReport.Reported;
    }

    static string Canonical(PartsDeclared declaration) => JsonSerializer.Serialize(declaration);

    static string Fingerprint(PartsDeclared declaration) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(Canonical(declaration))));

    EnvironmentName Environment()
    {
        if (options.Value.Environment != EnvironmentName.NotSet)
        {
            return options.Value.Environment;
        }

        return string.IsNullOrWhiteSpace(availability.EnvironmentName) ? EnvironmentName.NotSet : new(availability.EnvironmentName);
    }
}
