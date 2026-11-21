// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// The payload type names an envelope's <see cref="WatchdogEventEnvelope.Type"/> carries.
/// </summary>
public static class WatchdogEventTypes
{
    /// <summary>A check failed.</summary>
    public const string CheckFailed = "check-failed";

    /// <summary>A failing check passed again.</summary>
    public const string CheckRecovered = "check-recovered";

    /// <summary>A remedy was started.</summary>
    public const string RemedyAttempted = "remedy-attempted";

    /// <summary>A remedy's request succeeded.</summary>
    public const string RemedySucceeded = "remedy-succeeded";

    /// <summary>A remedy's request failed.</summary>
    public const string RemedyFailed = "remedy-failed";

    /// <summary>A remedy was not allowed to run.</summary>
    public const string RemedyDenied = "remedy-denied";

    /// <summary>A remedy's verification check passed.</summary>
    public const string RemedyVerified = "remedy-verified";

    /// <summary>Remedies stopped and a person must act.</summary>
    public const string RemedyEscalated = "remedy-escalated";

    /// <summary>The instance is alive.</summary>
    public const string Heartbeat = "heartbeat";

    static readonly Dictionary<Type, string> _names = new()
    {
        [typeof(CheckFailed)] = CheckFailed,
        [typeof(CheckRecovered)] = CheckRecovered,
        [typeof(RemedyAttempted)] = RemedyAttempted,
        [typeof(RemedySucceeded)] = RemedySucceeded,
        [typeof(RemedyFailed)] = RemedyFailed,
        [typeof(RemedyDenied)] = RemedyDenied,
        [typeof(RemedyVerified)] = RemedyVerified,
        [typeof(RemedyEscalated)] = RemedyEscalated,
        [typeof(Heartbeat)] = Heartbeat,
    };

    /// <summary>
    /// Gets every payload type name.
    /// </summary>
    public static IReadOnlyCollection<string> All => _names.Values;

    /// <summary>
    /// The type name of a payload type.
    /// </summary>
    /// <param name="payload">The payload type.</param>
    /// <returns>The name.</returns>
    /// <exception cref="ArgumentException">The type is not a watchdog payload.</exception>
    public static string Of(Type payload) =>
        _names.TryGetValue(payload, out var name) ? name : throw new ArgumentException($"{payload.Name} is not a watchdog event payload", nameof(payload));
}
