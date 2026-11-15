// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Understudy.Contracts.Backups;

/// <summary>
/// Published when a download link for a backup was handed out, so a backup that left the bucket leaves a record.
/// </summary>
/// <param name="Production">The Production the backup belongs to.</param>
/// <param name="BackupId">The backup that was downloaded.</param>
/// <param name="File">The file of the backup the link is for.</param>
/// <param name="DownloadedAt">When the link was handed out.</param>
[EventType]
[Public]
public record BackupDownloaded(ProductionName Production, BackupId BackupId, string File, DateTimeOffset DownloadedAt);
