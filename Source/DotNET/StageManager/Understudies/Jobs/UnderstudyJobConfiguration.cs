// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies.Jobs;

/// <summary>
/// What a backup job is told, as JSON in the <see cref="UnderstudyJobEnvironment.Configuration"/> variable. It holds nothing secret: credentials, connection strings
/// and the Vault token reach the job through their own variables, from Secrets.
/// </summary>
/// <param name="Kind">What the job does, one of <see cref="UnderstudyJobKinds"/>.</param>
/// <param name="Production">The name of the Production.</param>
/// <param name="ChronicleVersion">The Chronicle version the Production runs, recorded in the manifests.</param>
/// <param name="DatabasePrefix">The prefix Chronicle gives its databases.</param>
/// <param name="Databases">The declared database patterns.</param>
/// <param name="HasVault">Whether the Production keeps its keys in a Vault.</param>
/// <param name="Retention">The retention policy.</param>
/// <param name="Storage">Where database backups go.</param>
/// <param name="KeysStorage">Where key backups go, when they are taken.</param>
/// <param name="KeysMaxAgeDays">How long key backups are kept.</param>
/// <param name="VaultAddress">The address of the Vault.</param>
/// <param name="SystemDatabaseName">The name of the database holding the System event store, or empty for Chronicle's default.</param>
public record UnderstudyJobConfiguration(
    string Kind,
    string Production,
    string ChronicleVersion,
    string DatabasePrefix,
    IReadOnlyList<string> Databases,
    bool HasVault,
    RetentionPolicy Retention,
    UnderstudyStorage Storage,
    UnderstudyStorage? KeysStorage,
    int KeysMaxAgeDays,
    string VaultAddress,
    string SystemDatabaseName = "")
{
    static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Writes the configuration as JSON.
    /// </summary>
    /// <returns>The JSON.</returns>
    public string ToJson() => JsonSerializer.Serialize(this, _options);

    /// <summary>
    /// Reads a configuration.
    /// </summary>
    /// <param name="json">The JSON.</param>
    /// <returns>The configuration.</returns>
    /// <exception cref="InvalidDataException">Thrown when the content is not a configuration.</exception>
    public static UnderstudyJobConfiguration FromJson(string json) =>
        JsonSerializer.Deserialize<UnderstudyJobConfiguration>(json, _options) ?? throw new InvalidDataException("The job configuration is empty.");
}
