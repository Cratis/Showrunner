// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Understudy.Contracts.Rehearsals;

/// <summary>
/// Published when a rehearsal proved that a backup restores into a scratch environment and its checks passed.
/// </summary>
/// <param name="Production">The Production the backup belongs to.</param>
/// <param name="OperationId">The rehearsal.</param>
/// <param name="BackupId">The backup that was rehearsed.</param>
/// <param name="PassedAt">When the rehearsal passed.</param>
[EventType]
[Public]
public record RehearsalPassed(ProductionName Production, OperationId OperationId, BackupId BackupId, DateTimeOffset PassedAt);
