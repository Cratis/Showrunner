// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Discovery;

/// <summary>
/// The keys of one namespace, kept in Vault under <c>secret/{Store}/{namespace}/...</c>.
/// </summary>
/// <param name="EventStore">The event store.</param>
/// <param name="Namespace">The namespace.</param>
public record PlannedKeys(string EventStore, string Namespace)
{
    /// <summary>
    /// Gets the Vault path of the namespace, relative to the mount.
    /// </summary>
    public string Path => $"{EventStore}/{Namespace}";
}
