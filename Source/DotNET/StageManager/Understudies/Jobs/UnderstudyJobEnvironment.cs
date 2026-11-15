// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Jobs;

/// <summary>
/// The environment variables a backup job reads. Everything secret arrives through a variable fed from a Secret; none of them is ever logged.
/// </summary>
public static class UnderstudyJobEnvironment
{
    /// <summary>
    /// The <see cref="UnderstudyJobConfiguration"/> as JSON.
    /// </summary>
    public const string Configuration = "UNDERSTUDY_CONFIG";

    /// <summary>
    /// The connection string of the Production's MongoDB.
    /// </summary>
    public const string MongoDBConnection = "UNDERSTUDY_MONGODB_URI";

    /// <summary>
    /// The Chronicle connection string, which the job asks for the event stores and namespaces.
    /// </summary>
    public const string ChronicleConnection = "UNDERSTUDY_CHRONICLE_CONNECTION";

    /// <summary>
    /// The prefix of the variables that carry the credentials of the data bucket: <c>ACCESS_KEY</c>, <c>SECRET_KEY</c>, <c>ACCOUNT_NAME</c> and <c>ACCOUNT_KEY</c>.
    /// </summary>
    public const string StoragePrefix = "UNDERSTUDY_STORAGE_";

    /// <summary>
    /// The prefix of the variables that carry the credentials of the keys bucket.
    /// </summary>
    public const string KeysStoragePrefix = "UNDERSTUDY_KEYS_STORAGE_";

    /// <summary>
    /// The token the Vault is read with.
    /// </summary>
    public const string VaultToken = "UNDERSTUDY_VAULT_TOKEN";

    /// <summary>
    /// The customer's public certificate in PEM form.
    /// </summary>
    public const string KeysCertificate = "UNDERSTUDY_KEYS_CERTIFICATE";

    /// <summary>
    /// The directory a backup job dumps into before it uploads. Defaults to the temporary directory.
    /// </summary>
    public const string WorkDirectory = "UNDERSTUDY_WORK";

    /// <summary>
    /// The directory a Volume provider is mounted at.
    /// </summary>
    public const string VolumePath = "/backups";

    /// <summary>
    /// The suffix of the variable that carries the access key.
    /// </summary>
    public const string AccessKey = "ACCESS_KEY";

    /// <summary>
    /// The suffix of the variable that carries the secret key.
    /// </summary>
    public const string SecretKey = "SECRET_KEY";

    /// <summary>
    /// The suffix of the variable that carries the Azure storage account.
    /// </summary>
    public const string AccountName = "ACCOUNT_NAME";

    /// <summary>
    /// The suffix of the variable that carries the Azure account key.
    /// </summary>
    public const string AccountKey = "ACCOUNT_KEY";
}
