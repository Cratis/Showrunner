// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// A key of a Secret in the namespace of a <see cref="Production"/>. The value is mounted as a file and never copied into the spec, the status or an environment value.
/// </summary>
public class ProductionSecretReference
{
    /// <summary>
    /// The key a connection Secret uses when the reference names none.
    /// </summary>
    public const string DefaultKey = "connectionString";

    /// <summary>
    /// Gets or sets the name of the Secret, in the namespace of the Production.
    /// </summary>
    [Required]
    [Description("The name of the Secret, in the namespace of the Production.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key within the Secret.
    /// </summary>
    [Description("The key within the Secret. Defaults to connectionString.")]
    public string? Key { get; set; }

    /// <summary>
    /// Gets the key to read, with the default applied.
    /// </summary>
    /// <returns>The key.</returns>
    public string KeyOrDefault() => string.IsNullOrEmpty(Key) ? DefaultKey : Key;
}
