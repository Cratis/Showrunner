// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The Cratis Direct channel of a <see cref="Watchdog"/>: every alert is pushed to Direct's alert webhook, with the finding's key
/// as Direct's fingerprint so a repeat is the same alert.
/// </summary>
public sealed class WatchdogDirectArgs
{
    /// <summary>
    /// Gets the base URL of the Direct, for example <c>https://direct.example.com</c>.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Gets the bearer token of the alert source registered in Direct. It is written to a Secret as a Pulumi secret.
    /// </summary>
    public required Input<string> Token { get; init; }

    /// <summary>
    /// Gets the name to record the alerts under, or null to use the name of the source the token belongs to.
    /// </summary>
    public string? Source { get; init; }
}
