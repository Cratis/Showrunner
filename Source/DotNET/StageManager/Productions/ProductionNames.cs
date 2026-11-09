// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The names of everything a <see cref="Production"/> owns, all derived from the name of the Production.
/// </summary>
/// <param name="Production">The name of the Production.</param>
/// <param name="Namespace">The namespace of the Production.</param>
public readonly record struct ProductionNames(string Production, string Namespace)
{
    /// <summary>
    /// Gets the name of the Chronicle StatefulSet and of its headless Service.
    /// </summary>
    public string Chronicle => $"{Production}-chronicle";

    /// <summary>
    /// Gets the DNS name of the headless Service, which <c>chronicle+srv://</c> resolves.
    /// </summary>
    public string ChronicleHost => $"{Chronicle}.{Namespace}.svc.cluster.local";

    /// <summary>
    /// Gets the self-signed Issuer that signs the certificate authority.
    /// </summary>
    public string SelfSignedIssuer => $"{Production}-selfsigned";

    /// <summary>
    /// Gets the certificate authority: its Certificate, its Secret and the Issuer that signs with it.
    /// </summary>
    public string CertificateAuthority => $"{Production}-ca";

    /// <summary>
    /// Gets the Certificate of Chronicle and the Secret cert-manager writes it to, with its PKCS#12 keystore.
    /// </summary>
    public string ChronicleTls => $"{Production}-chronicle-tls";

    /// <summary>
    /// Gets the Secret holding the password of the PKCS#12 keystore.
    /// </summary>
    public string KeystorePassword => $"{Production}-chronicle-keystore";

    /// <summary>
    /// Gets the Secret holding the credentials Chronicle bootstraps: the administrator and the client of the applications.
    /// </summary>
    public string Credentials => $"{Production}-chronicle-credentials";

    /// <summary>
    /// Gets the connection Secret applications use.
    /// </summary>
    public string Connection => $"{Production}-chronicle-connection";

    /// <summary>
    /// Gets the PerconaServerMongoDB of a managed database.
    /// </summary>
    public string MongoDB => $"{Production}-mongodb";

    /// <summary>
    /// Gets the Secret the Percona operator keeps the users of a managed database in.
    /// </summary>
    public string MongoDBUsers => $"{Production}-mongodb-users";

    /// <summary>
    /// Gets the Secret holding the connection string of a managed database.
    /// </summary>
    public string MongoDBConnection => $"{Production}-mongodb-connection";

    /// <summary>
    /// Gets the host of the replica set of a managed database.
    /// </summary>
    public string MongoDBHost => $"{MongoDB}-rs0.{Namespace}.svc.cluster.local";

    /// <summary>
    /// Gets the StatefulSet, Service and Secret of a managed graph.
    /// </summary>
    public string Neo4j => $"{Production}-neo4j";

    /// <summary>
    /// Gets the bolt URI of a managed graph.
    /// </summary>
    public string Neo4jServer => $"bolt://{Neo4j}.{Namespace}.svc.cluster.local:7687";

    /// <summary>
    /// Gets the names of a Production.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The names.</returns>
    public static ProductionNames For(Production production) => new(production.Metadata.Name, production.Metadata.NamespaceProperty);
}
