// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// How the database behind a <see cref="Production"/> is provided.
/// </summary>
public class ProductionDatabase
{
    /// <summary>
    /// Gets or sets the storage engine: <c>MongoDB</c>, <c>PostgreSql</c> or <c>SqlServer</c>. The SQL engines are External only.
    /// </summary>
    [Description("MongoDB (the default), PostgreSql or SqlServer. PostgreSql and SqlServer are External only.")]
    public string Kind { get; set; } = StorageKinds.MongoDB;

    /// <summary>
    /// Gets or sets the mode: <c>Managed</c> or <c>External</c>.
    /// </summary>
    [Description("Managed or External.")]
    public string Mode { get; set; } = "External";

    /// <summary>
    /// Gets or sets the name of the Secret holding the connection string of an external database.
    /// </summary>
    [Description("The name of the Secret holding the connection string of an external database, under the key connectionString. Mounted as a file, never copied.")]
    public string ConnectionSecretName { get; set; } = string.Empty;
}
