// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Catalog;

/// <summary>
/// The record written beside the files of one backup run, as <c>manifest.json</c>. It is written last, and only when every file of the run was written,
/// so a run that has a manifest is complete; the manifests alone are enough to list every backup.
/// </summary>
/// <param name="SchemaVersion">The version of this manifest format.</param>
/// <param name="Production">The name of the Production.</param>
/// <param name="EventStore">The event store the run belongs to, or null for the Production-level run.</param>
/// <param name="Namespace">The namespace the run belongs to, or null for an event store or Production-level run.</param>
/// <param name="TakenAt">When the run started.</param>
/// <param name="ChronicleVersion">The Chronicle version the Production ran when the backup was taken, so a restore knows what it restores into.</param>
/// <param name="Retention">The retention tiers and maximum age in force when the backup was taken.</param>
/// <param name="Entries">The files of the run.</param>
public record BackupManifest(
    int SchemaVersion,
    string Production,
    string? EventStore,
    string? Namespace,
    DateTimeOffset TakenAt,
    string ChronicleVersion,
    RetentionPolicy Retention,
    IReadOnlyList<ManifestEntry> Entries)
{
    /// <summary>
    /// The version of the manifest format written today.
    /// </summary>
    public const int CurrentSchemaVersion = 1;

    /// <summary>
    /// The name of the manifest file.
    /// </summary>
    public const string FileName = "manifest.json";

    static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    /// <summary>
    /// Writes the manifest as JSON.
    /// </summary>
    /// <returns>The JSON.</returns>
    public byte[] ToJson() => JsonSerializer.SerializeToUtf8Bytes(this, _options);

    /// <summary>
    /// Reads a manifest.
    /// </summary>
    /// <param name="json">The JSON stream.</param>
    /// <returns>The manifest.</returns>
    /// <exception cref="InvalidDataException">Thrown when the content is not a manifest.</exception>
    public static BackupManifest FromJson(Stream json) =>
        JsonSerializer.Deserialize<BackupManifest>(json, _options) ?? throw new InvalidDataException("The manifest is empty.");
}
