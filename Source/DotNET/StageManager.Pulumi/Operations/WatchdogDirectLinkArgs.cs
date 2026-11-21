// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The link of a <see cref="Watchdog"/> to Cratis Direct's watchdog protocol: the watchdog enrolls with a one-time token, takes
/// Direct's signed configuration (the endpoint checks Direct defines), delivers its facts through a durable outbox and runs the
/// remedies Direct governs. It complements <see cref="WatchdogDirectArgs"/>, the lighter alert webhook, and the two can be used together.
/// </summary>
/// <remarks>
/// The component writes the enrollment token to the channels Secret as a Pulumi secret and grants the watchdog only what the link
/// needs: its own link ConfigMap and Secret, and - only for the Deployments named in <see cref="Remedies"/> - get and patch on exactly those.
/// </remarks>
public sealed class WatchdogDirectLinkArgs
{
    /// <summary>
    /// Gets the base URL of Direct, for example <c>https://direct.example.com</c>.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Gets the name of the watchdog instance enrolled in Direct (<c>/webhooks/watchdogs/{name}</c>).
    /// </summary>
    public required string InstanceName { get; init; }

    /// <summary>
    /// Gets the one-time enrollment token Direct showed when the instance was enrolled. It is written to a Secret as a Pulumi secret and
    /// only used until the first claim succeeded; the watchdog then keeps the instance credential in its own link Secret. After
    /// enrollment the token can be removed from the stack.
    /// </summary>
    public Input<string>? EnrollmentToken { get; init; }

    /// <summary>
    /// Gets the id of the Direct operational resource cluster observations are reported against, or null to derive a stable one from the instance name.
    /// </summary>
    public string? Resource { get; init; }

    /// <summary>
    /// Gets the Deployments the watchdog may restart for a Direct remedy of the same name. Without an entry a remedy is denied, never guessed at.
    /// </summary>
    public IList<WatchdogRemedyArgs> Remedies { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether a Direct remedy without an entry in <see cref="Remedies"/> may execute the HTTP request its signed
    /// configuration pins. Off by default: it makes the watchdog call whatever URL Direct's configuration names.
    /// </summary>
    public bool AllowHttpRemedies { get; init; }

    /// <summary>
    /// Gets the most facts the durable outbox keeps, or null for the default. It is stored in a ConfigMap, so it stays well under 1 MiB.
    /// </summary>
    public int? OutboxLimit { get; init; }
}
