// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// One named sink of a Production: the storage a name an application uses maps to.
/// </summary>
public sealed class SinkArgs
{
    /// <summary>
    /// Gets the name applications use. Unique within the Production, without regard to case.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the storage engine.
    /// </summary>
    public required SinkKind Kind { get; init; }

    /// <summary>
    /// Gets the existing Secret holding the connection: the connection string, or the bolt URI of a Neo4j server.
    /// </summary>
    public SecretReferenceArgs? ConnectionSecret { get; init; }

    /// <summary>
    /// Gets the graph of the Production a Neo4j sink uses instead of a connection Secret, such as <c>managed</c>.
    /// </summary>
    public string? GraphRef { get; init; }

    /// <summary>
    /// Builds the spec of the sink.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("name", Name)
            .PutEnum("kind", Kind)
            .Put("connectionSecretRef", ConnectionSecret is null ? null : SpecExtensions.SecretReference(ConnectionSecret.Name, ConnectionSecret.Key))
            .Put("graphRef", GraphRef);
}
