// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// One named sink of a <see cref="Production"/>: the storage a name an application uses maps to.
/// </summary>
public class ProductionSinkConfiguration
{
    /// <summary>
    /// Gets or sets the name applications use. Unique within the Production, without regard to case.
    /// </summary>
    [Required]
    [Description("The name applications use. Unique within the Production, without regard to case: letters and digits, separated by single hyphens or underscores.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the storage engine: <c>MongoDB</c>, <c>PostgreSql</c>, <c>SqlServer</c> or <c>Neo4j</c>.
    /// </summary>
    [Required]
    [Description("MongoDB, PostgreSql or SqlServer for read models, Neo4j for graphs.")]
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Secret holding the connection: the connection string, or the bolt URI of a Neo4j server.
    /// </summary>
    [Description("The Secret holding the connection: the connection string, or the bolt URI of a Neo4j server. Mounted as a file, never copied.")]
    public ProductionSecretReference? ConnectionSecretRef { get; set; }

    /// <summary>
    /// Gets or sets the graph of the Production a Neo4j sink uses instead of a connection Secret.
    /// </summary>
    [Description("For a Neo4j sink: the graph of the Production to use instead of connectionSecretRef, such as managed.")]
    public string? GraphRef { get; set; }
}
