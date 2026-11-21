// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The GitHub organization whose Actions schedules the <see cref="Watchdog"/> audits once a day. Naming it turns on the
/// <c>actions-audit</c> check. The token needs read-only access to the organization's repositories and their Actions, and nothing else.
/// </summary>
public sealed class WatchdogGitHubAuditArgs
{
    /// <summary>
    /// Gets the read-only token. It is written to a Secret as a Pulumi secret.
    /// </summary>
    public required Input<string> Token { get; init; }

    /// <summary>
    /// Gets the organization to audit, or null for <c>Cratis</c>.
    /// </summary>
    public string? Organization { get; init; }
}
