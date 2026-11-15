// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// Chronicle's database naming for MongoDB, mirrored from its <c>DatabaseNames</c>, so a Production's databases are found by asking Chronicle for its event stores
/// and namespaces and naming the databases they own, not by guessing from the names of the databases that exist.
/// </summary>
/// <param name="Prefix">The prefix Chronicle was configured with, or empty.</param>
/// <param name="SystemDatabaseName">The name Chronicle gives the System event store in its databases.</param>
public record ChronicleDatabaseNames(string Prefix = "", string SystemDatabaseName = ChronicleDatabaseNames.DefaultSystemDatabaseName)
{
    /// <summary>
    /// The name Chronicle gives the System event store in its databases when it is not configured otherwise.
    /// </summary>
    public const string DefaultSystemDatabaseName = "_system";

    /// <summary>
    /// The name of the System event store.
    /// </summary>
    public const string SystemEventStore = "System";

    /// <summary>
    /// The name of the default namespace.
    /// </summary>
    public const string DefaultNamespace = "Default";

    /// <summary>
    /// Gets the cluster database: <c>chronicle+main</c>.
    /// </summary>
    /// <returns>The name.</returns>
    public string Cluster() => $"{Prefix}chronicle+main";

    /// <summary>
    /// Gets the database of an event store: <c>{Store}+es</c>.
    /// </summary>
    /// <param name="eventStore">The event store.</param>
    /// <returns>The name.</returns>
    public string EventStore(string eventStore) => $"{Prefix}{NameFor(eventStore)}+es";

    /// <summary>
    /// Gets the events database of a namespace: <c>{Store}+es+{namespace}</c>.
    /// </summary>
    /// <param name="eventStore">The event store.</param>
    /// <param name="namespace">The namespace.</param>
    /// <returns>The name.</returns>
    public string Events(string eventStore, string @namespace) => $"{Prefix}{NameFor(eventStore)}+es+{@namespace}";

    /// <summary>
    /// Gets the read models database of a namespace: <c>{Store}</c> for the default namespace, otherwise <c>{Store}+{namespace}</c>.
    /// </summary>
    /// <param name="eventStore">The event store.</param>
    /// <param name="namespace">The namespace.</param>
    /// <returns>The name.</returns>
    public string ReadModels(string eventStore, string @namespace) =>
        string.Equals(@namespace, DefaultNamespace, StringComparison.Ordinal)
            ? $"{Prefix}{NameFor(eventStore)}"
            : $"{Prefix}{NameFor(eventStore)}+{@namespace}";

    string NameFor(string eventStore) =>
        string.Equals(eventStore, SystemEventStore, StringComparison.OrdinalIgnoreCase) ? SystemDatabaseName : eventStore;
}
