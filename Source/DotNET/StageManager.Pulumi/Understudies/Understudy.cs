// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Understudies;

/// <summary>
/// Declares a Stage Manager <c>Understudy</c>, scheduled backups of a Production's databases and, optionally, of its encryption
/// keys, with the Kubernetes Secrets it names when the credentials are given as values.
/// </summary>
/// <remarks>
/// The operator refuses a key backup whose bucket is the data bucket: whoever reads the data must not also read the keys.
/// </remarks>
public sealed class Understudy : ComponentResource
{
    /// <summary>
    /// The Pulumi resource type token of the component.
    /// </summary>
    public const string TypeToken = "cratis:showrunner:Understudy";

    /// <summary>
    /// Initializes a new instance of the <see cref="Understudy"/> class.
    /// </summary>
    /// <param name="name">The Pulumi name of the component.</param>
    /// <param name="args">The inputs.</param>
    /// <param name="options">The component options.</param>
    public Understudy(string name, UnderstudyArgs args, ComponentResourceOptions? options = null)
        : base(TypeToken, name, options)
    {
        KubernetesName = args.Name ?? name;
        var labels = args.Labels.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var secrets = new List<Resource>();

        void Write(string suffix, string secretName, IEnumerable<KeyValuePair<string, Input<string>>> data) =>
            secrets.Add(StageManagerSecrets.Create(this, args.Provider, $"{name}-{suffix}", secretName, args.Namespace, labels, data, args.DependsOn));

        string? WriteStorage(string suffix, UnderstudyStorageArgs storage)
        {
            var values = storage.CredentialValues();
            if (values.Count == 0)
            {
                return storage.CredentialsSecretName;
            }

            var secretName = storage.CredentialsSecretName ?? $"{KubernetesName}-{suffix}";
            Write(suffix, secretName, values);
            return secretName;
        }

        var storageSecretName = WriteStorage("backup-credentials", args.Storage);

        var spec = SpecExtensions.NewSpec()
            .Put("productionRef", args.ProductionName)
            .Put("schedule", args.Schedule)
            .Put("maxAgeDays", args.MaxAgeDays)
            .PutObject("retention", args.Retention?.ToSpec() ?? SpecExtensions.NewSpec())
            .Put("databasePrefix", args.DatabasePrefix)
            .PutList("databases", args.Databases)
            .Put("storage", args.Storage.ToSpec(storageSecretName))
            .Put("image", args.Image)
            .Put("workSize", args.WorkSize)
            .PutWhenTrue("suspend", args.Suspend);

        if (args.Keys is { } keys)
        {
            var keysSecretName = WriteStorage("key-backup-credentials", keys.Storage);
            var certificate = ResolveCertificate(keys, Write);
            spec.Put("keys", keys.ToSpec(keysSecretName, certificate));
        }

        StageManagerCustomResources.Declare(
            this,
            args.Provider,
            name,
            "Understudy",
            KubernetesName,
            args.Namespace,
            labels,
            spec,
            [.. secrets, .. args.DependsOn]);

        RegisterOutputs();
    }

    /// <summary>
    /// Gets the Kubernetes name of the Understudy.
    /// </summary>
    public string KubernetesName { get; }

    SecretReferenceArgs ResolveCertificate(
        UnderstudyKeysArgs keys,
        Action<string, string, IEnumerable<KeyValuePair<string, Input<string>>>> write)
    {
        var key = keys.CertificateSecretKey ?? UnderstudyKeysArgs.DefaultCertificateKey;
        if (keys.Certificate is not null)
        {
            var secretName = keys.CertificateSecretName ?? $"{KubernetesName}-key-backup-certificate";
            write("key-backup-certificate", secretName, [KeyValuePair.Create(key, keys.Certificate)]);
            return new SecretReferenceArgs { Name = secretName, Key = key };
        }

        return keys.CertificateSecretName is null
            ? throw new ArgumentException("Give the key backups a Certificate to write, or the name of the Secret that holds it.", nameof(keys))
            : new SecretReferenceArgs { Name = keys.CertificateSecretName, Key = keys.CertificateSecretKey };
    }
}
