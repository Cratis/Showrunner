// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The backup agent of a managed database, and optionally the object storage it writes physical backups to with the PBM <c>minio</c> storage type. An Understudy
/// additionally takes logical dumps of the database.
/// </summary>
public class ProductionDatabaseBackup
{
    /// <summary>
    /// The Percona Backup for MongoDB image the agent runs when none is given.
    /// </summary>
    public const string DefaultImage = "percona/percona-backup-mongodb:2.7.0";

    /// <summary>
    /// Gets or sets whether the backup agent runs.
    /// </summary>
    [Description("Runs the Percona Backup for MongoDB agent next to every member. Off by default.")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the image of the agent.
    /// </summary>
    [Description("The Percona Backup for MongoDB image. Defaults to a pinned version.")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the object storage Percona Backup for MongoDB writes to.
    /// </summary>
    [Description("The object storage the agent writes physical backups to, through the PBM minio storage type. Leave the bucket empty to attach none.")]
    public ProductionDatabaseBackupStorage Storage { get; set; } = new();

    /// <summary>
    /// Gets the image to run, with the default applied.
    /// </summary>
    /// <returns>The image.</returns>
    public string ImageOrDefault() => string.IsNullOrWhiteSpace(Image) ? DefaultImage : Image;
}
