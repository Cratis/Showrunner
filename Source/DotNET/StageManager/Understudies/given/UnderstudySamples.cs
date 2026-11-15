// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions;
using Cratis.Showrunner.StageManager.Productions.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.given;

/// <summary>
/// Understudies the specifications start from.
/// </summary>
public static class UnderstudySamples
{
    /// <summary>
    /// The name of the sample Understudy.
    /// </summary>
    public const string Name = "ledger-backups";

    /// <summary>
    /// An Understudy that backs the sample Production up to an S3-compatible bucket.
    /// </summary>
    /// <returns>The spec.</returns>
    public static UnderstudySpec Spec() => new()
    {
        ProductionRef = ProductionSamples.Name,
        Schedule = "15 * * * *",
        MaxAgeDays = 35,
        Databases = ["{eventStore}+jobs+{namespace}", "orleans"],
        Storage = new()
        {
            Provider = ObjectStorageProviders.S3,
            Endpoint = "https://objects.example.com",
            Region = "se-sto1",
            Bucket = "ledger-backups",
            PathStyle = true,
            CredentialsSecretName = "backup-credentials"
        }
    };

    /// <summary>
    /// The key backup settings of an Understudy: a bucket of its own, the customer's public certificate and a Vault token.
    /// </summary>
    /// <returns>The keys.</returns>
    public static UnderstudyKeys Keys() => new()
    {
        Schedule = "0 2 * * *",
        MaxAgeDays = 35,
        CertificateSecret = new() { Name = "customer-certificate" },
        VaultTokenSecret = new() { Name = "vault-reader" },
        Storage = new()
        {
            Provider = ObjectStorageProviders.S3,
            Endpoint = "https://objects.example.com",
            Region = "se-sto1",
            Bucket = "ledger-keys",
            PathStyle = true,
            CredentialsSecretName = "keys-credentials"
        }
    };

    /// <summary>
    /// An Understudy resource with the given spec.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>The Understudy.</returns>
    public static Understudy Understudy(UnderstudySpec spec) => new()
    {
        Metadata = new() { Name = Name, NamespaceProperty = ProductionSamples.Namespace, Uid = "d0a1f5f2-6b0e-4c39-9e43-2f5f0c4a1b7e", Generation = 3 },
        Spec = spec
    };

    /// <summary>
    /// The sample Production, which has written its connection Secret.
    /// </summary>
    /// <param name="vault">Whether it keeps its keys in a Vault.</param>
    /// <returns>The Production.</returns>
    public static Production Production(bool vault = false)
    {
        var production = ProductionSamples.Production(ProductionSamples.External());
        production.Status = new() { ConnectionSecretName = ProductionSamples.Names.Connection };
        if (vault)
        {
            production.Spec.Vault = new() { Mode = "External", Address = "https://vault.example:8200" };
        }

        return production;
    }
}
#endif
