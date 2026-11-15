// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Understudy.Contracts.Restores;

/// <summary>
/// Published when a restore failed.
/// </summary>
/// <param name="Production">The Production the backup was to be restored into.</param>
/// <param name="OperationId">The restore.</param>
/// <param name="BackupId">The backup that was to be restored.</param>
/// <param name="FailedAt">When the restore failed.</param>
/// <param name="Reason">Why it failed, without any secret value.</param>
[EventType]
[Public]
public record RestoreFailed(ProductionName Production, OperationId OperationId, BackupId BackupId, DateTimeOffset FailedAt, string Reason);
