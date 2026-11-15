// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies.given;

/// <summary>
/// Writes complete backup runs, as a backup job leaves them, into storage.
/// </summary>
public static class SeededBackups
{
    /// <summary>
    /// Writes the files of a run and its manifest.
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <param name="production">The Production.</param>
    /// <param name="eventStore">The event store, or null.</param>
    /// <param name="namespace">The namespace, or null.</param>
    /// <param name="takenAt">When the run started.</param>
    /// <param name="types">What the run holds.</param>
    /// <returns>The manifest.</returns>
    public static BackupManifest Run(InMemoryObjectStorage storage, string production, string? eventStore, string? @namespace, DateTimeOffset takenAt, params BackupType[] types)
    {
        var prefix = BackupLayout.RunPrefix(production, eventStore, @namespace, takenAt);
        var entries = new List<ManifestEntry>();
        foreach (var type in types)
        {
            var file = $"{type}.archive.gz";
            var content = Encoding.UTF8.GetBytes($"{type} of {eventStore}/{@namespace} at {takenAt:O}");
            storage.Seed(prefix + file, content, takenAt);
            entries.Add(new(type, $"{eventStore}-{type}", file, content.Length, "00"));
        }

        var manifest = new BackupManifest(BackupManifest.CurrentSchemaVersion, production, eventStore, @namespace, takenAt, "19.148.0", new RetentionPolicy(), entries);
        storage.Seed(BackupLayout.ManifestKey(prefix), manifest.ToJson(), takenAt);
        return manifest;
    }
}
#endif
