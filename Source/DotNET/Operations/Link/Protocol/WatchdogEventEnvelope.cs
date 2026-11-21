// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// One entry of an instance's outbox, as it crosses to Direct's inbox.
/// </summary>
/// <remarks>
/// <see cref="Sequence"/> is the instance's own monotonic outbox sequence, starting at 1 and never reused - even across
/// restarts. Direct deduplicates by (instance, sequence), so a batch may be delivered any number of times; a jump in
/// the sequence is recorded as a gap.
/// </remarks>
/// <param name="InstanceName">The instance that produced it.</param>
/// <param name="Sequence">The outbox sequence.</param>
/// <param name="EventId">The fact's identity.</param>
/// <param name="CausationId">What caused it - empty for nothing.</param>
/// <param name="Type">The payload type, one of <see cref="WatchdogEventTypes"/>.</param>
/// <param name="Occurred">When it happened on the instance.</param>
/// <param name="Payload">The payload, one of the payload records.</param>
public record WatchdogEventEnvelope(
    string InstanceName,
    long Sequence,
    Guid EventId,
    Guid CausationId,
    string Type,
    DateTimeOffset Occurred,
    JsonElement Payload)
{
    /// <summary>
    /// Wraps a payload.
    /// </summary>
    /// <typeparam name="TPayload">The payload type.</typeparam>
    /// <param name="instanceName">The instance.</param>
    /// <param name="sequence">The outbox sequence.</param>
    /// <param name="eventId">The fact's identity.</param>
    /// <param name="causationId">What caused it.</param>
    /// <param name="occurred">When it happened.</param>
    /// <param name="payload">The payload.</param>
    /// <returns>The envelope.</returns>
    public static WatchdogEventEnvelope For<TPayload>(string instanceName, long sequence, Guid eventId, Guid causationId, DateTimeOffset occurred, TPayload payload)
        where TPayload : notnull =>
        new(instanceName, sequence, eventId, causationId, WatchdogEventTypes.Of(typeof(TPayload)), occurred, JsonSerializer.SerializeToElement(payload, WatchdogJson.Options));

    /// <summary>
    /// Reads the payload.
    /// </summary>
    /// <typeparam name="TPayload">The payload type.</typeparam>
    /// <returns>The payload, or <see langword="null"/> when it is not of that type or cannot be read.</returns>
    public TPayload? PayloadAs<TPayload>()
        where TPayload : class
    {
        if (!string.Equals(Type, WatchdogEventTypes.Of(typeof(TPayload)), StringComparison.Ordinal))
        {
            return null;
        }

        try
        {
            return Payload.Deserialize<TPayload>(WatchdogJson.Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
