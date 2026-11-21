// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The managed MongoDB of a Production: a <see cref="PerconaServerMongoDB"/> with Cratis Infrastructure's settings as defaults, and the connection Chronicle reads from it.
/// </summary>
/// <remarks>
/// Images are pinned and never upgraded automatically. TLS between the members is disabled: with cert-manager rotating the certificate authority,
/// the Percona operator restarts the replica set in a loop. The members stay inside the cluster network.
/// </remarks>
public static class ManagedMongoDB
{
    /// <summary>
    /// The version of the custom resource the Percona operator reads.
    /// </summary>
    public const string CustomResourceVersion = "1.18.0";

    /// <summary>
    /// The state the Percona operator reports for a replica set that is ready.
    /// </summary>
    public const string ReadyState = "ready";

    /// <summary>
    /// The key of the user Chronicle connects as, in the users Secret the Percona operator keeps.
    /// </summary>
    public const string UserKey = "MONGODB_DATABASE_ADMIN_USER";

    /// <summary>
    /// The key of the password of that user.
    /// </summary>
    public const string PasswordKey = "MONGODB_DATABASE_ADMIN_PASSWORD";

    const string ReplicaSet = "rs0";

    /// <summary>
    /// Creates the PerconaServerMongoDB of a Production.
    /// </summary>
    /// <param name="production">The Production, with a Managed MongoDB database.</param>
    /// <returns>The resource.</returns>
    public static PerconaServerMongoDB Create(Production production)
    {
        var names = ProductionNames.For(production);
        var database = production.Spec.Database;

        var claim = new JsonObject
        {
            ["accessModes"] = new JsonArray("ReadWriteOnce"),
            ["resources"] = new JsonObject { ["requests"] = new JsonObject { ["storage"] = database.StorageSize } }
        };
        if (!string.IsNullOrWhiteSpace(database.StorageClassName))
        {
            claim["storageClassName"] = database.StorageClassName;
        }

        var replicaSet = new JsonObject
        {
            ["name"] = ReplicaSet,
            ["size"] = database.Members,
            ["affinity"] = new JsonObject { ["antiAffinityTopologyKey"] = database.OneMemberPerNode ? "kubernetes.io/hostname" : "none" },
            ["resources"] = new JsonObject { ["limits"] = new JsonObject { ["memory"] = database.MemoryLimit } },
            ["volumeSpec"] = new JsonObject { ["persistentVolumeClaim"] = claim }
        };
        if (database.NodeSelector.Count > 0)
        {
            replicaSet["nodeSelector"] = ToJson(database.NodeSelector);
        }

        // A replica set of fewer than three members is a development setup, which the Percona operator only accepts when told so.
        var unsafeFlags = new JsonObject { ["tls"] = true };
        if (database.Members < 3)
        {
            unsafeFlags["replsetSize"] = true;
        }

        return new()
        {
            Metadata = ProductionMetadata.Create(production, names.MongoDB, "mongodb"),
            Spec = new JsonObject
            {
                ["crVersion"] = CustomResourceVersion,
                ["image"] = database.ImageOrDefault(),
                ["imagePullPolicy"] = "IfNotPresent",
                ["unsafeFlags"] = unsafeFlags,
                ["tls"] = new JsonObject { ["mode"] = "disabled" },
                ["upgradeOptions"] = new JsonObject { ["apply"] = "disabled" },
                ["secrets"] = new JsonObject { ["users"] = names.MongoDBUsers },
                ["replsets"] = new JsonArray(replicaSet),
                ["sharding"] = new JsonObject { ["enabled"] = false },
                ["backup"] = Backup(database.Backup)
            }
        };
    }

    /// <summary>
    /// Gets the state the Percona operator reports for the database.
    /// </summary>
    /// <param name="database">The database as it is in the cluster.</param>
    /// <returns>The state, or null when none is reported yet.</returns>
    public static string? StateOf(PerconaServerMongoDB? database) => database?.Status?["state"]?.GetValue<string>();

    /// <summary>
    /// Composes the connection string Chronicle uses, from the user the Percona operator created.
    /// </summary>
    /// <param name="names">The names of the Production.</param>
    /// <param name="user">The user.</param>
    /// <param name="password">The password.</param>
    /// <returns>The connection string.</returns>
    public static string ConnectionString(ProductionNames names, string user, string password) =>
        $"mongodb://{Uri.EscapeDataString(user)}:{Uri.EscapeDataString(password)}@{names.MongoDBHost}:27017/?replicaSet={ReplicaSet}&authSource=admin";

    static JsonObject Backup(ProductionDatabaseBackup backup)
    {
        if (!backup.Enabled)
        {
            return new() { ["enabled"] = false };
        }

        var section = new JsonObject { ["enabled"] = true, ["image"] = backup.ImageOrDefault() };
        if (backup.Storage.IsConfigured())
        {
            AttachStorage(section, backup.Storage);
        }

        return section;
    }

    static void AttachStorage(JsonObject section, ProductionDatabaseBackupStorage storage)
    {
        var (host, secure) = storage.SplitEndpoint();
        var minio = new JsonObject
        {
            ["bucket"] = storage.Bucket,
            ["endpointUrl"] = host,
            ["secure"] = secure,
            ["forcePathStyle"] = true,
            ["prefix"] = storage.PrefixOrDefault(),
            ["credentialsSecret"] = storage.CredentialsSecretName
        };
        if (!string.IsNullOrWhiteSpace(storage.Region))
        {
            minio["region"] = storage.Region;
        }

        section["storages"] = new JsonObject
        {
            [ProductionDatabaseBackupStorage.StorageName] = new JsonObject { ["type"] = "minio", ["minio"] = minio }
        };
        var tasks = new JsonArray();
        foreach (var task in storage.TasksOrDefault())
        {
            tasks.Add(new JsonObject
            {
                ["name"] = task.Name,
                ["enabled"] = true,
                ["schedule"] = task.Schedule,
                ["keep"] = task.Keep,
                ["storageName"] = ProductionDatabaseBackupStorage.StorageName
            });
        }

        section["tasks"] = tasks;
    }

    static JsonObject ToJson(IDictionary<string, string> values)
    {
        var json = new JsonObject();
        foreach (var (key, value) in values.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            json[key] = value;
        }

        return json;
    }
}
