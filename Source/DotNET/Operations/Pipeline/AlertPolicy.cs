// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Pipeline;

/// <summary>
/// The rules that decide when an open finding is alerted: once when first seen, again after the re-alert interval, and, once it
/// has been open longer than the escalation threshold, once at the moment it crosses it and then at the shorter of the two
/// intervals.
/// </summary>
/// <param name="options">The timing options.</param>
public sealed class AlertPolicy(WatchdogOptions options)
{
    /// <summary>
    /// Appends when a finding was first seen, and for how long, to its text.
    /// </summary>
    /// <param name="text">The text of the finding.</param>
    /// <param name="firstSeen">When it was first seen.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The text with the open-since suffix.</returns>
    public static string WithOpenSince(string text, DateTimeOffset firstSeen, DateTimeOffset now) =>
        $"{text} (open since {firstSeen.UtcDateTime:yyyy-MM-dd HH:mm} UTC, {(int)Math.Floor((now - firstSeen).TotalHours)}h)";

    /// <summary>
    /// Decides whether a finding is due an alert now.
    /// </summary>
    /// <param name="now">The current time.</param>
    /// <param name="lastAlerted">When it was last alerted, or <see langword="null"/> when it never was.</param>
    /// <param name="firstSeen">When it was first seen.</param>
    /// <returns><see langword="true"/> when it should be alerted.</returns>
    public bool IsDue(DateTimeOffset now, DateTimeOffset? lastAlerted, DateTimeOffset firstSeen)
    {
        if (lastAlerted is not { } last)
        {
            return true;
        }

        if (IsEscalated(now, firstSeen))
        {
            var cooldown = options.RealertInterval < options.EscalateAfter ? options.RealertInterval : options.EscalateAfter;
            return last <= firstSeen + options.EscalateAfter || now - last >= cooldown;
        }

        return now - last > options.RealertInterval;
    }

    /// <summary>
    /// Decides whether a finding has been open longer than the escalation threshold. Exactly at the threshold is not escalated.
    /// </summary>
    /// <param name="now">The current time.</param>
    /// <param name="firstSeen">When it was first seen.</param>
    /// <returns><see langword="true"/> when it is escalated.</returns>
    public bool IsEscalated(DateTimeOffset now, DateTimeOffset firstSeen) => now - firstSeen > options.EscalateAfter;
}
