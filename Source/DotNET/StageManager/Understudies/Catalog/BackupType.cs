// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Catalog;

/// <summary>
/// What one backup file holds.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BackupType
{
    /// <summary>
    /// The events of one namespace: <c>{Store}+es+{namespace}</c>. Keys travel inside it when the Production has no Vault.
    /// </summary>
    Events = 0,

    /// <summary>
    /// The read models of one namespace: <c>{Store}</c> or <c>{Store}+{namespace}</c>.
    /// </summary>
    ReadModels = 1,

    /// <summary>
    /// A database the Understudy declares, named in its file. Per namespace, per event store or per Production, depending on its pattern.
    /// </summary>
    Database = 2,

    /// <summary>
    /// The database of one event store: <c>{Store}+es</c>.
    /// </summary>
    EventStore = 3,

    /// <summary>
    /// The cluster database of a Production: <c>chronicle+main</c>.
    /// </summary>
    Cluster = 4,

    /// <summary>
    /// The keys and erasure fences of one namespace, encrypted to the customer's certificate.
    /// </summary>
    Keys = 5,

    /// <summary>
    /// A whole-cluster disaster recovery snapshot of a Production.
    /// </summary>
    Snapshot = 6,

    /// <summary>
    /// A snapshot of the whole Vault of a Production, encrypted to the customer's certificate. Taken on demand.
    /// </summary>
    VaultSnapshot = 7
}
