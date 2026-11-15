// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Understudy.Contracts.Restores;

/// <summary>
/// Published when a restore finished and every database of the backup is back.
/// </summary>
/// <param name="Production">The Production the backup was restored into.</param>
/// <param name="OperationId">The restore.</param>
/// <param name="BackupId">The backup that was restored.</param>
/// <param name="CompletedAt">When the restore finished.</param>
[EventType]
[Public]
public record RestoreCompleted(ProductionName Production, OperationId OperationId, BackupId BackupId, DateTimeOffset CompletedAt);
