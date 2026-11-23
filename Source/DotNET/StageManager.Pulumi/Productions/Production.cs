// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// Declares a Stage Manager <c>Production</c>, Chronicle with its database, Vault and graph, with the Kubernetes Secrets it
/// names when the credentials are given as values instead of as the names of Secrets that exist already.
/// </summary>
/// <remarks>
/// The custom resource definitions are installed by the operator's Helm chart, so the operator must be installed first.
/// Every credential becomes a Pulumi secret. The custom resource waits for the Secrets it names, so the operator never
/// reconciles a half-made set.
/// </remarks>
public sealed class Production : ComponentResource
{
    /// <summary>
    /// The Pulumi resource type token of the component.
    /// </summary>
    public const string TypeToken = "cratis:showrunner:Production";

    /// <summary>
    /// Initializes a new instance of the <see cref="Production"/> class.
    /// </summary>
    /// <param name="name">The Pulumi name of the component.</param>
    /// <param name="args">The inputs.</param>
    /// <param name="options">The component options.</param>
    public Production(string name, ProductionArgs args, ComponentResourceOptions? options = null)
        : base(TypeToken, name, options)
    {
        KubernetesName = args.Name ?? name;
        var labels = args.Labels.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var secrets = new List<Resource>();

        void Write(string suffix, string secretName, params (string Key, Input<string> Value)[] data) =>
            secrets.Add(StageManagerSecrets.Create(
                this,
                args.Provider,
                $"{name}-{suffix}",
                secretName,
                args.Namespace,
                labels,
                data.Select(entry => KeyValuePair.Create(entry.Key, entry.Value)),
                args.DependsOn));

        SecretReferenceArgs? Resolve(string suffix, string defaultKey, string? secretName, string? secretKey, Input<string>? value)
        {
            if (value is null)
            {
                return secretName is null ? null : new SecretReferenceArgs { Name = secretName, Key = secretKey };
            }

            var written = secretName ?? $"{KubernetesName}-{suffix}";
            var key = secretKey ?? defaultKey;
            Write(suffix, written, (key, value));
            return new SecretReferenceArgs { Name = written, Key = key };
        }

        var database = args.Database;
        var connectionSecretName = database.ConnectionSecretName;
        if (database.ConnectionString is not null)
        {
            connectionSecretName ??= $"{KubernetesName}-database";
            Write("database", connectionSecretName, ("connectionString", database.ConnectionString));
        }

        var vault = args.Vault;
        var tokenSecret = vault is null ? null : Resolve("vault-token", ProductionVaultArgs.DefaultTokenKey, vault.TokenSecretName, vault.TokenSecretKey, vault.Token);

        var dial = args.Federation?.Dial;
        var enrollmentSecret = dial is null ? null : Resolve("enrollment-code", FederationDialArgs.DefaultEnrollmentCodeKey, dial.EnrollmentCodeSecretName, dial.EnrollmentCodeSecretKey, dial.EnrollmentCode);

        var backup = database.Backup?.Storage;
        var backupCredentialsSecretName = backup?.CredentialsSecretName;
        if (backup is { HasCredentials: true })
        {
            if (backup.AccessKeyId is null || backup.SecretAccessKey is null)
            {
                throw new ArgumentException("Give both AccessKeyId and SecretAccessKey of the backup storage, or neither and name an existing Secret.", nameof(args));
            }

            backupCredentialsSecretName ??= $"{KubernetesName}-backup-credentials";
            Write(
                "backup-credentials",
                backupCredentialsSecretName,
                (DatabaseBackupStorageArgs.AccessKeyIdKey, backup.AccessKeyId),
                (DatabaseBackupStorageArgs.SecretAccessKeyKey, backup.SecretAccessKey));
        }

        var spec = SpecExtensions.NewSpec()
            .Put("version", args.Version)
            .Put("replicas", args.Replicas)
            .Put("database", database.ToSpec(connectionSecretName, backupCredentialsSecretName))
            .Put("vault", vault?.ToSpec(tokenSecret))
            .Put("federation", args.Federation?.ToSpec(enrollmentSecret))
            .Put("graph", args.Graph?.ToSpec())
            .Put("scheduling", args.Scheduling?.ToSpec())
            .Put("resources", args.Resources?.ToSpec())
            .Put("sinks", args.Sinks?.ToSpec());

        StageManagerCustomResources.Declare(
            this,
            args.Provider,
            name,
            "Production",
            KubernetesName,
            args.Namespace,
            labels,
            spec,
            [.. secrets, .. args.DependsOn]);

        RegisterOutputs();
    }

    /// <summary>
    /// Gets the Kubernetes name of the Production, which Performances and Understudies reference.
    /// </summary>
    public string KubernetesName { get; }
}
