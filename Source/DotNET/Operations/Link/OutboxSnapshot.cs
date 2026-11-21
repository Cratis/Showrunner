// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// The durable form of the outbox.
/// </summary>
/// <param name="LastAssigned">The last sequence handed out - never reused, even across restarts.</param>
/// <param name="Watermark">The highest sequence Direct has durably taken.</param>
/// <param name="Baselined">Whether the outbox has asked Direct for its watermark since it was created.</param>
/// <param name="Evicted">How many entries were evicted before Direct took them, ever.</param>
/// <param name="Entries">The entries Direct has not taken yet, in ascending sequence.</param>
public sealed record OutboxSnapshot(long LastAssigned, long Watermark, bool Baselined, long Evicted, IReadOnlyList<WatchdogEventEnvelope> Entries)
{
    /// <summary>
    /// Gets the outbox of a link that has never run.
    /// </summary>
    public static OutboxSnapshot Empty { get; } = new(0, 0, false, 0, []);
}
