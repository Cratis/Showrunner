// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Brings a Production into the cluster, or says why it cannot be. Nothing is applied until every input the Production names is in hand,
/// and Chronicle is applied only once its certificate and database connection exist.
/// </summary>
/// <param name="cluster">The cluster.</param>
/// <param name="preparation">The preparation that gates the reconciliation.</param>
/// <param name="time">The time provider.</param>
/// <param name="logger">The logger.</param>
public class ProductionReconciler(IProductionCluster cluster, ProductionPreparation preparation, TimeProvider time, ILogger<ProductionReconciler> logger)
{
    /// <summary>
    /// Reconciles the Production and writes its status.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The phase the Production ended in.</returns>
    public async Task<ProductionPhase> ReconcileAsync(Production production, CancellationToken cancellationToken)
    {
        production.Status ??= new();
        var status = new ProductionStatusBuilder(production, time);
        var prepared = await preparation.PrepareAsync(production, status, cancellationToken);
        if (prepared is not null)
        {
            await ApplyAsync(production, prepared, status, cancellationToken);
        }

        status.Write();
        await cluster.UpdateStatusAsync(production, cancellationToken);
        logger.ProductionPhase(production.Metadata.NamespaceProperty, production.Metadata.Name, status.Phase);
        return status.Phase;
    }

    async Task ApplyAsync(Production production, PreparedProduction prepared, ProductionStatusBuilder status, CancellationToken cancellationToken)
    {
        var names = ProductionNames.For(production);

        // From here on the Production runs with this database and graph; a spec asking for others is refused.
        production.Status.Database = ProductionPreparation.DatabaseOf(production.Spec);
        production.Status.Graph = ProductionPreparation.GraphOf(production.Spec);
        production.Status.ChronicleAddress = ChronicleConnectionSecret.AddressOf(names);

        var certificateAuthority = await ApplyCertificatesAsync(production, names, status, cancellationToken);
        var credentials = await EnsureCredentialsAsync(production, names, cancellationToken);
        var databaseReady = await ApplyDatabaseAsync(production, names, status, cancellationToken);
        var graphReady = await ApplyGraphAsync(production, names, status, cancellationToken);

        if (certificateAuthority is not null)
        {
            await cluster.ApplyAsync(ChronicleConnectionSecret.Create(production, credentials, certificateAuthority), cancellationToken);
            production.Status.ConnectionSecretName = names.Connection;
            status.Set(ProductionConditions.ConnectionAvailable, true, "Written", $"Applications connect with the Secret '{names.Connection}'.");
        }
        else
        {
            status.Set(ProductionConditions.ConnectionAvailable, false, "WaitingForCertificates", "The connection Secret is written once the certificate authority is issued.");
        }

        // Chronicle cannot start without its certificate and its database connection, so it waits for both.
        var chronicleReady = false;
        if (certificateAuthority is not null && databaseReady)
        {
            await cluster.ApplyAsync(ChronicleWorkload.Service(production), cancellationToken);
            await cluster.ApplyAsync(ChronicleWorkload.StatefulSet(production, prepared.Kernel), cancellationToken);
            var readiness = await cluster.GetStatefulSetReadinessAsync(names.Namespace, names.Chronicle, cancellationToken);
            chronicleReady = readiness?.IsReady ?? false;
            var ready = chronicleReady ? production.Spec.Replicas : readiness?.Ready ?? 0;
            var message = $"{ready} of {production.Spec.Replicas} replica(s) of Chronicle {production.Spec.Version} are ready.";
            status.Set(ProductionConditions.ChronicleReady, chronicleReady, chronicleReady ? "Ready" : "NotReady", message);
        }
        else
        {
            status.Set(ProductionConditions.ChronicleReady, false, "WaitingForDependencies", "Chronicle is applied once its certificate is issued and its database is ready.");
        }

        status.Applied(chronicleReady && databaseReady && graphReady);
    }

    async Task<string?> ApplyCertificatesAsync(Production production, ProductionNames names, ProductionStatusBuilder status, CancellationToken cancellationToken)
    {
        var password = await cluster.GetSecretValueAsync(names.Namespace, names.KeystorePassword, ProductionCertificates.KeystorePasswordKey, cancellationToken);
        if (password is null)
        {
            await cluster.ApplyAsync(ProductionMetadata.Secret(production, names.KeystorePassword, "certificates", new Dictionary<string, string> { [ProductionCertificates.KeystorePasswordKey] = GeneratedSecret.New() }), cancellationToken);
        }

        foreach (var resource in ProductionCertificates.Create(production))
        {
            await (resource switch
            {
                CertManagerIssuer issuer => cluster.ApplyAsync(issuer, cancellationToken),
                CertManagerCertificate certificate => cluster.ApplyAsync(certificate, cancellationToken),
                _ => Task.CompletedTask
            });
        }

        var keystore = await cluster.GetSecretValueAsync(names.Namespace, names.ChronicleTls, ProductionCertificates.KeystoreKey, cancellationToken);
        var certificateAuthority = await cluster.GetSecretValueAsync(names.Namespace, names.ChronicleTls, ProductionCertificates.CertificateAuthorityKey, cancellationToken);
        var issued = keystore is not null && certificateAuthority is not null;
        status.Set(
            ProductionConditions.CertificatesReady,
            issued,
            issued ? "Issued" : "NotIssued",
            issued ? $"Chronicle's certificate is in the Secret '{names.ChronicleTls}'." : $"cert-manager has not issued Chronicle's certificate into the Secret '{names.ChronicleTls}' yet.");
        return issued ? certificateAuthority : null;
    }

    async Task<ChronicleCredentials> EnsureCredentialsAsync(Production production, ProductionNames names, CancellationToken cancellationToken)
    {
        var clientId = await cluster.GetSecretValueAsync(names.Namespace, names.Credentials, ChronicleCredentials.ClientIdKey, cancellationToken);
        var clientSecret = await cluster.GetSecretValueAsync(names.Namespace, names.Credentials, ChronicleCredentials.ClientSecretKey, cancellationToken);
        var adminPassword = await cluster.GetSecretValueAsync(names.Namespace, names.Credentials, ChronicleCredentials.AdminPasswordKey, cancellationToken);
        if (clientId is not null && clientSecret is not null && adminPassword is not null)
        {
            return new(clientId, clientSecret, adminPassword);
        }

        var credentials = ChronicleCredentials.Generate();
        await cluster.ApplyAsync(ProductionMetadata.Secret(production, names.Credentials, "chronicle", credentials.ToSecretValues()), cancellationToken);
        return credentials;
    }

    async Task<bool> ApplyDatabaseAsync(Production production, ProductionNames names, ProductionStatusBuilder status, CancellationToken cancellationToken)
    {
        var database = production.Spec.Database;
        if (database.IsExternal())
        {
            status.Set(ProductionConditions.DatabaseReady, true, "External", $"Chronicle reads its {database.Kind} connection from the Secret '{database.ConnectionSecretName}'.");
            return true;
        }

        await cluster.ApplyAsync(ManagedMongoDB.Create(production), cancellationToken);
        var state = await cluster.GetMongoDBStateAsync(names.Namespace, names.MongoDB, cancellationToken);
        if (!string.Equals(state, ManagedMongoDB.ReadyState, StringComparison.Ordinal))
        {
            status.Set(ProductionConditions.DatabaseReady, false, "NotReady", $"The managed MongoDB '{names.MongoDB}' with {database.Members} member(s) is {state ?? "not reporting a state yet"}.");
            return false;
        }

        var user = await cluster.GetSecretValueAsync(names.Namespace, names.MongoDBUsers, ManagedMongoDB.UserKey, cancellationToken);
        var password = await cluster.GetSecretValueAsync(names.Namespace, names.MongoDBUsers, ManagedMongoDB.PasswordKey, cancellationToken);
        if (user is null || password is null)
        {
            status.Set(ProductionConditions.DatabaseReady, false, "UsersNotFound", $"The managed MongoDB is ready, but its users Secret '{names.MongoDBUsers}' has no database administrator yet.");
            return false;
        }

        var connection = ManagedMongoDB.ConnectionString(names, user, password);
        await cluster.ApplyAsync(ProductionMetadata.Secret(production, names.MongoDBConnection, "mongodb", new Dictionary<string, string> { [ProductionSecretReference.DefaultKey] = connection }), cancellationToken);
        status.Set(ProductionConditions.DatabaseReady, true, "Ready", $"The managed MongoDB '{names.MongoDB}' with {database.Members} member(s) is ready.");
        return true;
    }

    async Task<bool> ApplyGraphAsync(Production production, ProductionNames names, ProductionStatusBuilder status, CancellationToken cancellationToken)
    {
        var graph = production.Spec.Graph;
        switch (graph.Mode)
        {
            case ProductionModes.Managed:
                var password = await cluster.GetSecretValueAsync(names.Namespace, names.Neo4j, ProductionGraph.PasswordKey, cancellationToken) ?? GeneratedSecret.New();
                await cluster.ApplyAsync(ManagedGraph.Secret(production, password), cancellationToken);
                await cluster.ApplyAsync(ManagedGraph.Service(production), cancellationToken);
                await cluster.ApplyAsync(ManagedGraph.StatefulSet(production), cancellationToken);
                var ready = (await cluster.GetStatefulSetReadinessAsync(names.Namespace, names.Neo4j, cancellationToken))?.IsReady ?? false;
                status.Set(ProductionConditions.GraphReady, ready, ready ? "Ready" : "NotReady", ready ? $"The managed Neo4j Community '{names.Neo4j}' is ready." : $"The managed Neo4j Community '{names.Neo4j}' is not ready yet.");
                return ready;

            case ProductionModes.External:
                status.Set(ProductionConditions.GraphReady, true, "External", $"Chronicle reads the {graph.Edition} graph's address and password from the Secret '{graph.CredentialsSecretName}'.");
                return true;

            default:
                status.Set(ProductionConditions.GraphReady, true, "NoGraph", "The Production has no graph; the kernel's Neo4j settings are left unset.");
                return true;
        }
    }
}
