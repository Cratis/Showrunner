// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Cratis.Choreography.Availability;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Availability.Declaration;

/// <summary>
/// Reports what the product's model declares to the operator as <see cref="AvailabilityDeclared"/>, once per change.
/// What was last reported is read from the outbox, so a restart, a second instance or another build that declares the same
/// reports nothing again. It reads the <see cref="IAvailabilityManifest"/> and nothing else: the engine decides what is
/// available, and this only says what it was told.
/// </summary>
/// <param name="manifest">The <see cref="IAvailabilityManifest"/> of the product.</param>
/// <param name="options">The <see cref="GuestListOptions"/>.</param>
/// <param name="availability">The <see cref="AvailabilityOptions"/> of the engine.</param>
/// <param name="logger">The <see cref="ILogger{AvailabilityReporter}"/>.</param>
public class AvailabilityReporter(IAvailabilityManifest manifest, IOptions<GuestListOptions> options, AvailabilityOptions availability, ILogger<AvailabilityReporter> logger)
{
    /// <summary>
    /// Describes the manifest as the product reports it now.
    /// </summary>
    /// <returns>The declaration.</returns>
    public AvailabilityDeclared Declaration() =>
        AvailabilityDeclarations.From(manifest.Current, options.Value.Product, Environment(), Version(), availability.GrantAllDeclaredAudiences);

    /// <summary>
    /// Reports the declaration to the outbox of an event store, unless the operator already has it.
    /// </summary>
    /// <param name="eventStore">The product's event store.</param>
    /// <returns>What reporting came to.</returns>
    /// <exception cref="AvailabilityNotReported">The declaration could not be appended.</exception>
    public async Task<AvailabilityReport> Report(IEventStore eventStore)
    {
        var current = manifest.Current;
        if (current.Audiences.Count == 0 && current.Blocks.Count == 0 && current.Gates.Count == 0 && current.ContextValues.Count == 0 && current.Personas.Count == 0)
        {
            logger.NothingDeclared(options.Value.Product);
            return AvailabilityReport.NothingDeclared;
        }

        var outbox = eventStore.GetEventSequence(EventSequenceId.Outbox);
        var declared = eventStore.EventTypes.GetEventTypeFor(typeof(AvailabilityDeclared));
        var reported = await outbox.GetForEventSourceIdAndEventTypes(options.Value.Product.Value, [declared]);
        var last = reported.Count > 0 ? reported[^1] : null;
        var declaration = Declaration();

        if (last?.Content is AvailabilityDeclared previous && AvailabilityDeclarations.AreSame(previous, declaration))
        {
            logger.AvailabilityUnchanged(options.Value.Product);
            return AvailabilityReport.Unchanged;
        }

        var key = new IdempotencyKey($"guestlist-availability-{AvailabilityDeclarations.Fingerprint(declaration)}-{last?.Context.SequenceNumber.Value.ToString() ?? "first"}");
        using var routing = string.IsNullOrWhiteSpace(options.Value.OperatorAudience) ? null : EventTags.Audience(options.Value.OperatorAudience);
        var result = await outbox.AppendOnce(options.Value.Product.Value, declaration, key);
        if (result.WasAlreadyAppended || result.HasIdempotencyConflicts)
        {
            // Another instance reported the same declaration over the same last one.
            logger.AvailabilityUnchanged(options.Value.Product);
            return AvailabilityReport.Unchanged;
        }

        if (!result.IsSuccess)
        {
            throw new AvailabilityNotReported(options.Value.Product, result);
        }

        logger.AvailabilityReported(options.Value.Product, declaration.Audiences.Count(), declaration.Gates.Count());
        return AvailabilityReport.Reported;
    }

    EnvironmentName Environment()
    {
        if (options.Value.Environment != EnvironmentName.NotSet)
        {
            return options.Value.Environment;
        }

        return string.IsNullOrWhiteSpace(availability.EnvironmentName) ? EnvironmentName.NotSet : new(availability.EnvironmentName);
    }

    ModelVersion Version() =>
        options.Value.Version != ModelVersion.NotSet
            ? options.Value.Version
            : new(Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty);
}
