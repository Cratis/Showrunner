// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Understudies;

/// <summary>
/// An object storage bucket an Understudy writes to. The credentials are never part of the custom resource: they live in a Secret.
/// </summary>
public sealed class UnderstudyStorageArgs
{
    /// <summary>
    /// The key of the Secret that holds the access key of an S3 compatible provider.
    /// </summary>
    public const string AccessKeyKey = "accessKey";

    /// <summary>
    /// The key of the Secret that holds the secret key of an S3 compatible provider.
    /// </summary>
    public const string SecretKeyKey = "secretKey";

    /// <summary>
    /// The key of the Secret that holds the storage account of Azure Blob.
    /// </summary>
    public const string AccountNameKey = "accountName";

    /// <summary>
    /// The key of the Secret that holds the account key of Azure Blob.
    /// </summary>
    public const string AccountKeyKey = "accountKey";

    /// <summary>
    /// Gets the provider. Defaults to <see cref="ObjectStorageProvider.S3"/>.
    /// </summary>
    public ObjectStorageProvider Provider { get; init; } = ObjectStorageProvider.S3;

    /// <summary>
    /// Gets the service endpoint of an S3 compatible provider, such as <c>https://example.upcloudobjects.com</c>. Empty means AWS.
    /// </summary>
    public Input<string>? Endpoint { get; init; }

    /// <summary>
    /// Gets the region of an S3 compatible provider.
    /// </summary>
    public Input<string>? Region { get; init; }

    /// <summary>
    /// Gets the bucket (S3) or container (Azure Blob).
    /// </summary>
    public Input<string>? Bucket { get; init; }

    /// <summary>
    /// Gets a value indicating whether S3 addresses the bucket in the path instead of the host name. MinIO and most
    /// self-hosted providers need it.
    /// </summary>
    public bool PathStyle { get; init; }

    /// <summary>
    /// Gets the service URL of Azure Blob, such as an Azurite endpoint. Empty means the public cloud endpoint of the account.
    /// </summary>
    public Input<string>? BlobServiceUrl { get; init; }

    /// <summary>
    /// Gets the name of the PersistentVolumeClaim a <see cref="ObjectStorageProvider.Volume"/> provider writes to.
    /// </summary>
    public string? VolumeClaimName { get; init; }

    /// <summary>
    /// Gets the name of the Secret that holds the credentials. It is created, under this name, when the credential values
    /// are given; otherwise it must already exist.
    /// </summary>
    public string? CredentialsSecretName { get; init; }

    /// <summary>
    /// Gets the access key of an S3 provider, written as a Pulumi secret to the credentials Secret.
    /// </summary>
    public Input<string>? AccessKey { get; init; }

    /// <summary>
    /// Gets the secret key of an S3 provider, written as a Pulumi secret to the credentials Secret.
    /// </summary>
    public Input<string>? SecretKey { get; init; }

    /// <summary>
    /// Gets the storage account of an Azure Blob provider, written as a Pulumi secret to the credentials Secret.
    /// </summary>
    public Input<string>? AccountName { get; init; }

    /// <summary>
    /// Gets the account key of an Azure Blob provider, written as a Pulumi secret to the credentials Secret.
    /// </summary>
    public Input<string>? AccountKey { get; init; }

    /// <summary>
    /// Gets the credential values to write for the provider, in the keys the operator reads.
    /// </summary>
    /// <returns>The values by key, empty when none were given.</returns>
    internal IReadOnlyList<KeyValuePair<string, Input<string>>> CredentialValues()
    {
        var values = new List<KeyValuePair<string, Input<string>>>();
        if (Provider == ObjectStorageProvider.AzureBlob)
        {
            AddPair(values, AccountNameKey, AccountName, AccountKeyKey, AccountKey);
        }
        else
        {
            AddPair(values, AccessKeyKey, AccessKey, SecretKeyKey, SecretKey);
        }

        return values;
    }

    /// <summary>
    /// Builds the spec of the storage.
    /// </summary>
    /// <param name="credentialsSecretName">The name of the Secret holding the credentials, if any.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(string? credentialsSecretName) =>
        SpecExtensions.NewSpec()
            .PutEnum("provider", Provider)
            .Put("endpoint", Endpoint)
            .Put("region", Region)
            .Put("bucket", Bucket)
            .PutWhenTrue("pathStyle", PathStyle)
            .Put("blobServiceUrl", BlobServiceUrl)
            .Put("volumeClaimName", VolumeClaimName)
            .Put("credentialsSecretName", credentialsSecretName);

    static void AddPair(List<KeyValuePair<string, Input<string>>> values, string firstKey, Input<string>? first, string secondKey, Input<string>? second)
    {
        if (first is null && second is null)
        {
            return;
        }

        if (first is null || second is null)
        {
            throw new ArgumentException($"Give both {firstKey} and {secondKey} values of the storage credentials, or neither and name an existing Secret.");
        }

        values.Add(KeyValuePair.Create(firstKey, first));
        values.Add(KeyValuePair.Create(secondKey, second));
    }
}
