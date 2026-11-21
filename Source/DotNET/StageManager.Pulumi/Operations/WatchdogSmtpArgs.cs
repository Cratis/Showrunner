// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The email channel of a <see cref="Watchdog"/>. The connection is STARTTLS, then login.
/// </summary>
public sealed class WatchdogSmtpArgs
{
    /// <summary>
    /// Gets the host name of the SMTP server.
    /// </summary>
    public required string Server { get; init; }

    /// <summary>
    /// Gets the port of the SMTP server, or null for 587.
    /// </summary>
    public int? Port { get; init; }

    /// <summary>
    /// Gets the user to log in as. It is written to a Secret.
    /// </summary>
    public required Input<string> Username { get; init; }

    /// <summary>
    /// Gets the password to log in with. It is written to a Secret as a Pulumi secret.
    /// </summary>
    public required Input<string> Password { get; init; }

    /// <summary>
    /// Gets the display name of the sender.
    /// </summary>
    public required string FromName { get; init; }

    /// <summary>
    /// Gets the address of the sender.
    /// </summary>
    public required string From { get; init; }

    /// <summary>
    /// Gets the address of the recipient, or several separated by commas.
    /// </summary>
    public required string To { get; init; }
}
