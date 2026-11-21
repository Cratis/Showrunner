// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// What the watchdog last did through the link. It is written to the job log and to the link's status ConfigMap, and holds no
/// credential, key or secret.
/// </summary>
/// <param name="Instance">The instance name in Direct.</param>
/// <param name="Connection">How the link stood.</param>
/// <param name="PassAt">When the pass ended.</param>
/// <param name="Epoch">The fencing epoch held.</param>
/// <param name="AppliedRevision">The configuration revision applied, zero for none.</param>
/// <param name="ConfigurationExpiresAt">When the configuration held lapses, or <see langword="null"/> for none.</param>
/// <param name="OutboxDepth">How many outbox entries Direct has not taken.</param>
/// <param name="Watermark">The highest outbox sequence Direct took.</param>
/// <param name="Evicted">How many entries were evicted from the bounded outbox before Direct took them.</param>
/// <param name="Delivered">How many events this pass delivered.</param>
/// <param name="Remedies">What remedies did this pass.</param>
/// <param name="Problems">What went wrong, without secrets.</param>
public sealed record LinkStatus(
    string Instance,
    LinkConnection Connection,
    DateTimeOffset PassAt,
    long Epoch,
    long AppliedRevision,
    DateTimeOffset? ConfigurationExpiresAt,
    int OutboxDepth,
    long Watermark,
    long Evicted,
    int Delivered,
    IReadOnlyList<string> Remedies,
    IReadOnlyList<string> Problems)
{
    /// <summary>
    /// Gets a one-line summary for the job log.
    /// </summary>
    /// <returns>The summary.</returns>
    public string Summary() =>
        $"{Connection.ToString().ToLowerInvariant()}; epoch {Epoch}; revision {AppliedRevision}; outbox {OutboxDepth} pending, watermark {Watermark}, {Delivered} delivered, {Evicted} evicted; " +
        $"{Remedies.Count} remedy action(s){(Problems.Count == 0 ? string.Empty : "; problems: " + string.Join(" | ", Problems))}";
}
