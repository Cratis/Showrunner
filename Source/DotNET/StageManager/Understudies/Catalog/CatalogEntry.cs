// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Catalog;

/// <summary>
/// One backup run in the catalog.
/// </summary>
/// <param name="Manifest">The manifest of the run.</param>
/// <param name="ManifestKey">The key of the manifest, from which the run's files are found.</param>
/// <param name="Verdict">Whether the retention policy keeps the run, and by which tiers.</param>
public record CatalogEntry(BackupManifest Manifest, string ManifestKey, RetentionVerdict Verdict)
{
    /// <summary>
    /// Gets the prefix of the run's files.
    /// </summary>
    public string RunPrefix => ManifestKey[..^BackupManifest.FileName.Length];
}
