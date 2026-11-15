// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Jobs;

/// <summary>
/// One scope whose backup failed. Nothing of it was kept.
/// </summary>
/// <param name="EventStore">The event store, or null.</param>
/// <param name="Namespace">The namespace, or null.</param>
/// <param name="Reason">What went wrong. It carries no secret value.</param>
public record BackupFailure(string? EventStore, string? Namespace, string Reason);
