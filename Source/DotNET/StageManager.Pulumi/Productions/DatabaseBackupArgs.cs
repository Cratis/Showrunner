// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The backup agent Percona runs next to every member of a managed database. This is where an Understudy attaches the
/// storage it backs up to.
/// </summary>
public sealed class DatabaseBackupArgs
{
    /// <summary>
    /// Gets a value indicating whether the backup agent runs. Defaults to true: naming backup settings means wanting them.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the image of the backup agent, or null for the operator's default.
    /// </summary>
    public string? Image { get; init; }

    /// <summary>
    /// Gets the object storage the backups are written to, or null to attach none.
    /// </summary>
    public DatabaseBackupStorageArgs? Storage { get; init; }

    /// <summary>
    /// Builds the spec of the backup agent.
    /// </summary>
    /// <param name="credentialsSecretName">The name of the Secret holding the storage credentials, if any.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(string? credentialsSecretName) =>
        SpecExtensions.NewSpec()
            .Put("enabled", Enabled)
            .Put("image", Image)
            .Put("storage", Storage?.ToSpec(credentialsSecretName));
}
