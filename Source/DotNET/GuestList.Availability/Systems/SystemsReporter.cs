// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Choreography.Availability;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.GuestList.Availability.Declaration;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Availability.Systems;

/// <summary>
/// Reports the systems that call the product to the operator as <see cref="SystemsDeclared"/>, once per change. What was last
/// reported is read from the outbox, so a restart, a second instance or another build that knows the same systems reports
/// nothing again. A product that has never reported and knows no systems reports nothing; one that stops knowing systems it
/// reported before reports the empty list, which replaces the earlier one.
/// </summary>
/// <param name="systems">The <see cref="IKnownSystems"/> of the product.</param>
/// <param name="options">The <see cref="GuestListOptions"/>.</param>
/// <param name="availability">The <see cref="AvailabilityOptions"/> of the engine.</param>
/// <param name="logger">The <see cref="ILogger{SystemsReporter}"/>.</param>
public class SystemsReporter(IKnownSystems systems, IOptions<GuestListOptions> options, AvailabilityOptions availability, ILogger<SystemsReporter> logger)
{
    /// <summary>
    /// Describes the systems as the product reports them now: ordered by client id, so another order is the same declaration.
    /// </summary>
    /// <returns>The declaration.</returns>
    public SystemsDeclared Declaration() =>
        new(
            options.Value.Product,
            Environment(),
            [.. systems.Current.Select(system => system with { Name = system.Name ?? string.Empty }).DistinctBy(system => system.ClientId.Value, StringComparer.Ordinal).OrderBy(system => system.ClientId.Value, StringComparer.Ordinal)]);

    /// <summary>
    /// Reports the systems to the outbox of an event store, unless the operator already has them.
    /// </summary>
    /// <param name="eventStore">The product's event store.</param>
    /// <returns>What reporting came to.</returns>
    /// <exception cref="AvailabilityNotReported">The declaration could not be appended.</exception>
    public async Task<AvailabilityReport> Report(IEventStore eventStore)
    {
        var outbox = eventStore.GetEventSequence(EventSequenceId.Outbox);
        var declared = eventStore.EventTypes.GetEventTypeFor(typeof(SystemsDeclared));
        var reported = await outbox.GetForEventSourceIdAndEventTypes(options.Value.Product.Value, [declared]);
        var last = reported.Count > 0 ? reported[^1] : null;
        var declaration = Declaration();

        if (last is null && !declaration.Systems.Any())
        {
            return AvailabilityReport.NothingDeclared;
        }

        if (last?.Content is SystemsDeclared previous && Canonical(previous) == Canonical(declaration))
        {
            logger.SystemsUnchanged(options.Value.Product);
            return AvailabilityReport.Unchanged;
        }

        var key = new IdempotencyKey($"guestlist-systems-{Fingerprint(declaration)}-{last?.Context.SequenceNumber.Value.ToString() ?? "first"}");
        using var routing = string.IsNullOrWhiteSpace(options.Value.OperatorAudience) ? null : EventTags.Audience(options.Value.OperatorAudience);
        var result = await outbox.AppendOnce(options.Value.Product.Value, declaration, key);
        if (result.WasAlreadyAppended || result.HasIdempotencyConflicts)
        {
            logger.SystemsUnchanged(options.Value.Product);
            return AvailabilityReport.Unchanged;
        }

        if (!result.IsSuccess)
        {
            throw new AvailabilityNotReported(options.Value.Product, result);
        }

        logger.SystemsReported(options.Value.Product, declaration.Systems.Count());
        return AvailabilityReport.Reported;
    }

    static string Canonical(SystemsDeclared declaration) => JsonSerializer.Serialize(declaration);

    static string Fingerprint(SystemsDeclared declaration) =>
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
