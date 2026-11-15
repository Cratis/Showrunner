// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// One database a rule claims.
/// </summary>
/// <param name="Type">What the backup is.</param>
/// <param name="EventStore">The event store it belongs to, or null for a Production-level database.</param>
/// <param name="Namespace">The namespace it belongs to, or null when it belongs to an event store or to the Production.</param>
/// <param name="Database">The name of the database.</param>
/// <param name="Present">Whether the database exists. A claimed database that does not exist yet, such as the read models of a namespace that has none, is not dumped.</param>
public record PlannedBackup(BackupType Type, string? EventStore, string? Namespace, string Database, bool Present);
