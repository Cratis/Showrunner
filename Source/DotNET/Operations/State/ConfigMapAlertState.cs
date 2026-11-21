// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.State;

/// <summary>
/// An <see cref="IAlertState"/> kept in the <c>state</c> key of a ConfigMap, in the format the Python watchdog used.
/// </summary>
/// <param name="store">The ConfigMap store.</param>
/// <param name="configMap">The name of the ConfigMap, <c>cluster-health-watchdog-state</c> by convention.</param>
public sealed class ConfigMapAlertState(IConfigMapStore store, string configMap) : IAlertState
{
    /// <summary>
    /// The key in the ConfigMap's data the state is stored under.
    /// </summary>
    public const string Key = "state";

    /// <inheritdoc/>
    public async Task<AlertStateSnapshot> Load(CancellationToken cancellationToken) =>
        AlertStateJson.Parse(await store.Read(configMap, Key, cancellationToken));

    /// <inheritdoc/>
    public Task Save(AlertStateSnapshot snapshot, CancellationToken cancellationToken) =>
        store.Write(configMap, Key, AlertStateJson.Serialize(snapshot), cancellationToken);
}
