// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// The latest backup of one type, as the catalog holds it.
/// </summary>
public class LastBackupStatus
{
    /// <summary>
    /// Gets or sets what the backup is.
    /// </summary>
    [Description("What the backup is: Events, ReadModels, Database, EventStore, Cluster or Keys.")]
    public BackupType Type { get; set; }

    /// <summary>
    /// Gets or sets when the backup was taken.
    /// </summary>
    [Description("When the backup was taken.")]
    public DateTimeOffset TakenAt { get; set; }

    /// <summary>
    /// Gets or sets the size of the backup files of that type in the latest run.
    /// </summary>
    [Description("The size of the files of that type in the latest run, in bytes, summed over the namespaces.")]
    public long SizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the number of namespaces, event stores or databases the latest run of that type covers.
    /// </summary>
    [Description("How many files of that type the latest run of each namespace, event store or Production holds, summed.")]
    public int Files { get; set; }
}
