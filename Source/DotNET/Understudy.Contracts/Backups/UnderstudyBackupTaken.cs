// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Understudy.Contracts.Backups;

/// <summary>
/// Published when a backup run was written completely: every file is in the bucket and its manifest is the last thing written.
/// </summary>
/// <param name="Production">The Production that was backed up.</param>
/// <param name="BackupId">The run, which is also its place in the backup catalog.</param>
/// <param name="EventStore">The event store the run belongs to, or empty for a Production-level run.</param>
/// <param name="Namespace">The namespace the run belongs to, or empty for an event store or Production-level run.</param>
/// <param name="TakenAt">When the run started.</param>
/// <param name="Types">What the run holds: Events, ReadModels, Database, EventStore, Cluster, Keys or VaultSnapshot.</param>
/// <param name="SizeBytes">The size of the files of the run.</param>
[EventType]
[Public]
public record UnderstudyBackupTaken(ProductionName Production, BackupId BackupId, string EventStore, string Namespace, DateTimeOffset TakenAt, IEnumerable<string> Types, long SizeBytes);
