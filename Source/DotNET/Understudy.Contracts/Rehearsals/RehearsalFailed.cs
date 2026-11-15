// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Understudy.Contracts.Rehearsals;

/// <summary>
/// Published when a rehearsal could not restore a backup or one of its checks failed.
/// </summary>
/// <param name="Production">The Production the backup belongs to.</param>
/// <param name="OperationId">The rehearsal.</param>
/// <param name="BackupId">The backup that was rehearsed.</param>
/// <param name="FailedAt">When the rehearsal failed.</param>
/// <param name="Reason">Why it failed, without any secret value.</param>
[EventType]
[Public]
public record RehearsalFailed(ProductionName Production, OperationId OperationId, BackupId BackupId, DateTimeOffset FailedAt, string Reason);
