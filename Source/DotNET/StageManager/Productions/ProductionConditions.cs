// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The types of the conditions a <see cref="Production"/> reports.
/// </summary>
public static class ProductionConditions
{
    /// <summary>
    /// The spec has no problem outside its named sinks.
    /// </summary>
    public const string SpecValid = "SpecValid";

    /// <summary>
    /// The key storage the spec asks for is supported.
    /// </summary>
    public const string VaultSupported = "VaultSupported";

    /// <summary>
    /// The database kind and mode are the ones the Production runs with.
    /// </summary>
    public const string DatabaseModeUnchanged = "DatabaseModeUnchanged";

    /// <summary>
    /// The graph mode and edition are the ones the Production runs with.
    /// </summary>
    public const string GraphModeUnchanged = "GraphModeUnchanged";

    /// <summary>
    /// cert-manager is installed.
    /// </summary>
    public const string CertManagerInstalled = "CertManagerInstalled";

    /// <summary>
    /// The certificate authority and the certificate of Chronicle are issued.
    /// </summary>
    public const string CertificatesReady = "CertificatesReady";

    /// <summary>
    /// The database is ready and Chronicle has its connection.
    /// </summary>
    public const string DatabaseReady = "DatabaseReady";

    /// <summary>
    /// The graph is ready, or the Production has none.
    /// </summary>
    public const string GraphReady = "GraphReady";

    /// <summary>
    /// Every Chronicle replica is ready and runs the version asked for.
    /// </summary>
    public const string ChronicleReady = "ChronicleReady";

    /// <summary>
    /// The connection Secret applications use is written.
    /// </summary>
    public const string ConnectionAvailable = "ConnectionAvailable";

    /// <summary>
    /// The prefix of the condition of each named sink, followed by its name.
    /// </summary>
    public const string SinkConfiguredPrefix = "SinkConfigured.";

    /// <summary>
    /// Gets the condition type of a named sink.
    /// </summary>
    /// <param name="sink">The name of the sink.</param>
    /// <returns>The condition type.</returns>
    public static string SinkConfigured(string sink) => $"{SinkConfiguredPrefix}{sink}";
}
