// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The Discord channel of a <see cref="Watchdog"/>.
/// </summary>
public sealed class WatchdogDiscordArgs
{
    /// <summary>
    /// Gets the webhook URL. It is a capability, so it is written to a Secret as a Pulumi secret.
    /// </summary>
    public required Input<string> WebhookUrl { get; init; }
}
