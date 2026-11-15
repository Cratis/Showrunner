// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// Works out what to back up from what Chronicle says it holds and what the database server says exists. A database is claimed by the first rule that names it:
/// the cluster database, the database of an event store, the events and read models of a namespace, then the declared databases.
/// </summary>
public static class BackupPlanner
{
    /// <summary>
    /// The token of a declared database pattern that stands for the name of the event store.
    /// </summary>
    public const string EventStoreToken = "{eventStore}";

    /// <summary>
    /// The token of a declared database pattern that stands for the name of the namespace.
    /// </summary>
    public const string NamespaceToken = "{namespace}";

    static readonly HashSet<string> _serverOwned = new(StringComparer.Ordinal) { "admin", "local", "config" };

    /// <summary>
    /// Plans the backups.
    /// </summary>
    /// <param name="topology">The event stores and namespaces Chronicle holds.</param>
    /// <param name="existing">The names of the databases that exist.</param>
    /// <param name="options">The options.</param>
    /// <returns>The plan.</returns>
    public static BackupPlan Plan(ChronicleTopology topology, IEnumerable<string> existing, BackupPlanOptions options)
    {
        var present = new HashSet<string>(existing, StringComparer.Ordinal);
        var claimed = new HashSet<string>(StringComparer.Ordinal);
        var backups = new List<PlannedBackup>();
        var keys = new List<PlannedKeys>();

        void Claim(BackupType type, string? eventStore, string? @namespace, string database)
        {
            if (claimed.Add(database))
            {
                backups.Add(new(type, eventStore, @namespace, database, present.Contains(database)));
            }
        }

        Claim(BackupType.Cluster, null, null, options.Names.Cluster());
        foreach (var eventStore in topology.EventStores)
        {
            Claim(BackupType.EventStore, eventStore.Name, null, options.Names.EventStore(eventStore.Name));
            foreach (var @namespace in eventStore.Namespaces)
            {
                Claim(BackupType.Events, eventStore.Name, @namespace, options.Names.Events(eventStore.Name, @namespace));
                Claim(BackupType.ReadModels, eventStore.Name, @namespace, options.Names.ReadModels(eventStore.Name, @namespace));
                if (options.HasVault)
                {
                    keys.Add(new(eventStore.Name, @namespace));
                }
            }
        }

        foreach (var pattern in options.DeclaredDatabases)
        {
            ClaimDeclared(pattern, topology, Claim);
        }

        var unclaimed = present.Where(name => !claimed.Contains(name) && !_serverOwned.Contains(name)).Order(StringComparer.Ordinal).ToList();
        var serverOwned = present.Where(_serverOwned.Contains).Order(StringComparer.Ordinal).ToList();
        return new(backups, keys, unclaimed, serverOwned);
    }

    static void ClaimDeclared(string pattern, ChronicleTopology topology, Action<BackupType, string?, string?, string> claim)
    {
        var perStore = pattern.Contains(EventStoreToken, StringComparison.Ordinal);
        var perNamespace = pattern.Contains(NamespaceToken, StringComparison.Ordinal);
        if (!perStore && !perNamespace)
        {
            claim(BackupType.Database, null, null, pattern);
            return;
        }

        foreach (var eventStore in topology.EventStores)
        {
            var named = pattern.Replace(EventStoreToken, eventStore.Name, StringComparison.Ordinal);
            if (!perNamespace)
            {
                claim(BackupType.Database, eventStore.Name, null, named);
                continue;
            }

            foreach (var @namespace in eventStore.Namespaces)
            {
                claim(BackupType.Database, eventStore.Name, @namespace, named.Replace(NamespaceToken, @namespace, StringComparison.Ordinal));
            }
        }
    }
}
