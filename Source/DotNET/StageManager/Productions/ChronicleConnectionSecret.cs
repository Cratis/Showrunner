// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The connection Secret of a Production: everything an application needs to reach its Chronicle, and the only thing it needs.
/// </summary>
public static class ChronicleConnectionSecret
{
    /// <summary>
    /// The key of the connection string, which a Performance mounts.
    /// </summary>
    public const string ConnectionStringKey = "connectionString";

    /// <summary>
    /// Gets the address of Chronicle, without credentials: its headless Service, resolved through DNS SRV records.
    /// </summary>
    /// <param name="names">The names of the Production.</param>
    /// <returns>The address.</returns>
    public static string AddressOf(ProductionNames names) => $"chronicle+srv://{names.ChronicleHost}";

    /// <summary>
    /// Gets the connection string applications use: the address with the client credentials.
    /// </summary>
    /// <param name="names">The names of the Production.</param>
    /// <param name="credentials">The credentials.</param>
    /// <returns>The connection string.</returns>
    public static string ConnectionStringOf(ProductionNames names, ChronicleCredentials credentials) =>
        $"chronicle+srv://{Uri.EscapeDataString(credentials.ClientId)}:{Uri.EscapeDataString(credentials.ClientSecret)}@{names.ChronicleHost}";

    /// <summary>
    /// Creates the connection Secret.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="credentials">The credentials of the client applications connect as.</param>
    /// <param name="certificateAuthority">The certificate authority Chronicle's certificate chains to, in PEM.</param>
    /// <returns>The Secret.</returns>
    public static V1Secret Create(Production production, ChronicleCredentials credentials, string certificateAuthority)
    {
        var names = ProductionNames.For(production);
        return ProductionMetadata.Secret(production, names.Connection, "chronicle", new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ConnectionStringKey] = ConnectionStringOf(names, credentials),
            [ChronicleCredentials.ClientIdKey] = credentials.ClientId,
            [ChronicleCredentials.ClientSecretKey] = credentials.ClientSecret,
            [ProductionCertificates.CertificateAuthorityKey] = certificateAuthority
        });
    }
}
