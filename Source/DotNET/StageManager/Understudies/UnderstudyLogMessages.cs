// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// Log messages of the Understudy reconciliation and of its backup jobs. None of them carries a secret value, a key or a tenant's data.
/// </summary>
public static partial class UnderstudyLogMessages
{
    [LoggerMessage(LogLevel.Information, "Understudy {Namespace}/{Understudy} is {Phase}")]
    internal static partial void UnderstudyPhase(this ILogger logger, string @namespace, string understudy, UnderstudyPhase phase);

    [LoggerMessage(LogLevel.Error, "Reconciling Understudy {Namespace}/{Understudy} failed")]
    internal static partial void ReconciliationFailed(this ILogger logger, string @namespace, string understudy, Exception exception);

    [LoggerMessage(LogLevel.Information, "Backup plan of {Production}: {Databases} database(s) to back up, {Unclaimed} unclaimed")]
    internal static partial void PlanMade(this ILogger logger, string production, int databases, int unclaimed);

    [LoggerMessage(LogLevel.Information, "Backed up {Production}/{EventStore}/{Namespace}: {Files} file(s), {Bytes} bytes")]
    internal static partial void ScopeBackedUp(this ILogger logger, string production, string eventStore, string @namespace, int files, long bytes);

    [LoggerMessage(LogLevel.Error, "Backup of {Production}/{EventStore}/{Namespace} failed: {Reason}")]
    internal static partial void ScopeFailed(this ILogger logger, string production, string eventStore, string @namespace, string reason);

    [LoggerMessage(LogLevel.Information, "Pruned {Production}: {Kept} kept, {Pruned} pruned, {Orphans} orphaned run(s) removed")]
    internal static partial void Pruned(this ILogger logger, string production, int kept, int pruned, int orphans);

    [LoggerMessage(LogLevel.Error, "Key backups of {Production} are refused: {Reason}")]
    internal static partial void KeyBackupsRefused(this ILogger logger, string production, string reason);

    [LoggerMessage(LogLevel.Information, "Backed up the keys of {Production}/{EventStore}/{Namespace}")]
    internal static partial void KeysBackedUp(this ILogger logger, string production, string eventStore, string @namespace);

    [LoggerMessage(LogLevel.Error, "The job kind '{Kind}' is not supported. Use backup, keys or vault-snapshot")]
    internal static partial void JobKindNotSupported(this ILogger logger, string kind);

    [LoggerMessage(LogLevel.Error, "The {Kind} job failed ({Type}): {Message}")]
    internal static partial void JobFailed(this ILogger logger, string kind, string type, string message);

    [LoggerMessage(LogLevel.Warning, "The metrics endpoint could not listen on port {Port} ({Type}); metrics are not served")]
    internal static partial void MetricsEndpointNotStarted(this ILogger logger, int port, string type);

    [LoggerMessage(LogLevel.Information, "Took the Vault snapshot of {Production}")]
    internal static partial void VaultSnapshotTaken(this ILogger logger, string production);
}
