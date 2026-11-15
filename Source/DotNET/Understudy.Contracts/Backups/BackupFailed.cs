// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Understudy.Contracts.Backups;

/// <summary>
/// Published when a backup job failed. A failed backup is never reported as taken: nothing of it was kept.
/// </summary>
/// <param name="Production">The Production that was to be backed up.</param>
/// <param name="OperationId">The job that failed.</param>
/// <param name="FailedAt">When the job failed.</param>
/// <param name="Kind">What the job did: backup, keys or vault-snapshot.</param>
/// <param name="Reason">Why it failed, without any secret value.</param>
[EventType]
[Public]
public record BackupFailed(ProductionName Production, OperationId OperationId, DateTimeOffset FailedAt, string Kind, string Reason);
