// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The Chronicle whose observers the <see cref="Watchdog"/> watches (failed partitions and quarantined observers). Naming it turns on
/// the <c>chronicle-observers</c> check. The client needs read access to the event stores' observation queries.
/// </summary>
public sealed class WatchdogChronicleArgs
{
    /// <summary>
    /// Gets the Chronicle API address (its TLS API port).
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Gets the client id of the application client the watchdog signs in with.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets the client secret. It is written to a Secret as a Pulumi secret.
    /// </summary>
    public required Input<string> ClientSecret { get; init; }

    /// <summary>
    /// Gets the PEM of the certificate authority Chronicle's certificate chains to, when it is not a public one, or null. It is written
    /// to the Secret next to the client secret.
    /// </summary>
    public Input<string>? CertificateAuthority { get; init; }
}
