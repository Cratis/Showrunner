// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The graph of a Production bound to Cratis.Arc.Neo4j through its <c>Cratis:Neo4j</c> section.
/// </summary>
/// <param name="Server">The bolt URI of a managed graph, which the operator knows and is not a secret, or null when it is only in a Secret.</param>
/// <param name="ServerSecret">The Secret key holding the bolt URI of an external graph, or null for a managed graph.</param>
/// <param name="UserName">The user to authenticate with.</param>
/// <param name="Password">The Secret key holding the password.</param>
/// <param name="DatabaseMode"><c>Single</c> or <c>PerNamespace</c>, as the kernel writes the graph.</param>
/// <param name="DatabasePrefix">The prefix of the database names on an Enterprise server, or null.</param>
/// <param name="EventStore">The event store whose graph is read: the first of the application, or null when it names none.</param>
/// <param name="SharedCredential">Whether the application reads with the same credential as the kernel, as on Community.</param>
public record GraphBinding(
    string? Server,
    ResolvedSecretRef? ServerSecret,
    string UserName,
    ResolvedSecretRef Password,
    string DatabaseMode,
    string? DatabasePrefix,
    string? EventStore,
    bool SharedCredential)
{
    /// <summary>
    /// Gets the Secret keys the graph is read from.
    /// </summary>
    public IEnumerable<ResolvedSecretRef> Secrets => ServerSecret is null ? [Password] : [ServerSecret, Password];
}
