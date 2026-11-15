// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Showrunner.StageManager.Understudies.Catalog;

/// <summary>
/// Where the files of a backup run live: <c>{production}/{eventStore}/{namespace}/{takenAt}/</c>. A run that belongs to an event store has <c>_</c> for the namespace,
/// and a run that belongs to the Production has <c>_</c> for both. <c>takenAt</c> is UTC as <c>yyyyMMddTHHmmssZ</c>, so keys sort by time.
/// </summary>
public static class BackupLayout
{
    /// <summary>
    /// The segment that stands for a level the run does not belong to.
    /// </summary>
    public const string NotApplicable = "_";

    const string TakenAtFormat = "yyyyMMdd'T'HHmmss'Z'";

    /// <summary>
    /// Gets the prefix of everything a Production has.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The prefix, ending in a slash.</returns>
    public static string ProductionPrefix(string production) => $"{production}/";

    /// <summary>
    /// Gets the prefix of one run.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="eventStore">The event store, or null.</param>
    /// <param name="namespace">The namespace, or null.</param>
    /// <param name="takenAt">When the run started.</param>
    /// <returns>The prefix, ending in a slash.</returns>
    public static string RunPrefix(string production, string? eventStore, string? @namespace, DateTimeOffset takenAt) =>
        $"{production}/{eventStore ?? NotApplicable}/{@namespace ?? NotApplicable}/{FormatTakenAt(takenAt)}/";

    /// <summary>
    /// Gets the key of the manifest of one run.
    /// </summary>
    /// <param name="runPrefix">The prefix of the run.</param>
    /// <returns>The key.</returns>
    public static string ManifestKey(string runPrefix) => $"{runPrefix}{BackupManifest.FileName}";

    /// <summary>
    /// Formats the time a run started.
    /// </summary>
    /// <param name="takenAt">The time.</param>
    /// <returns>The segment.</returns>
    public static string FormatTakenAt(DateTimeOffset takenAt) => takenAt.UtcDateTime.ToString(TakenAtFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// Reads the run prefix a key belongs to, if it belongs to one.
    /// </summary>
    /// <param name="key">The key of an object.</param>
    /// <returns>The prefix of the run, ending in a slash, or null when the key is not in a run.</returns>
    public static string? RunPrefixOf(string key)
    {
        var segments = key.Split('/');
        return segments.Length >= 5 ? string.Join('/', segments.Take(4)) + "/" : null;
    }
}
