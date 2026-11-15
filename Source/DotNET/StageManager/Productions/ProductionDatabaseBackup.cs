// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The backup agent of a managed database. It only makes the database ready to be backed up; where backups go is up to the Understudy.
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
    /// Gets the image to run, with the default applied.
    /// </summary>
    /// <returns>The image.</returns>
    public string ImageOrDefault() => string.IsNullOrWhiteSpace(Image) ? DefaultImage : Image;
}
