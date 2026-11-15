// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Creates the HashiCorp Vault that Stage Manager runs for a <see cref="Production"/> whose <c>vault.mode</c> is Managed, and the token Chronicle authenticates to it with.
/// </summary>
/// <remarks>
/// The development tier is one node on file storage. The production tier is three nodes on Raft storage that tolerate the loss of one.
/// A helper container next to every Vault node initializes Vault once, keeps it unsealed (unless a cloud key service or a transit Vault does that),
/// mounts KV v2 at <c>secret/</c> and writes the token Chronicle uses to a Secret. In the production tier that token can reach only Chronicle's own key paths.
/// </remarks>
public static class ManagedVault
{
    /// <summary>
    /// The port of the Vault API.
    /// </summary>
    public const int Port = 8200;

    /// <summary>
    /// The port the nodes of a Raft cluster talk to each other on.
    /// </summary>
    public const int ClusterPort = 8201;

    /// <summary>
    /// The key of the Secret holding the token Chronicle authenticates with.
    /// </summary>
    public const string TokenKey = "token";

    /// <summary>
    /// The key of the Secret of the bootstrap helper that holds the root token.
    /// </summary>
    public const string RootTokenKey = "rootToken";

    /// <summary>
    /// The key of the Secret of the bootstrap helper that holds the key that unseals Vault, when the Secret does.
    /// </summary>
    public const string UnsealKeyKey = "unsealKey";

    /// <summary>
    /// The key of the Secret of the bootstrap helper that holds the recovery key, when a cloud key service or a transit Vault unseals Vault.
    /// </summary>
    public const string RecoveryKeyKey = "recoveryKey";

    /// <summary>
    /// The name of the file in the ConfigMap that holds the Vault configuration.
    /// </summary>
    public const string ConfigFile = "vault.hcl";

    /// <summary>
    /// The name of the file in the ConfigMap that holds the bootstrap script.
    /// </summary>
    public const string BootstrapFile = "bootstrap.sh";

    /// <summary>
    /// The annotation that rolls the nodes when the configuration changes.
    /// </summary>
    public const string ConfigRevisionAnnotation = "showrunner.cratis.io/vault-config-revision";

    /// <summary>
    /// The image of the bootstrap helper: it needs curl, jq and kubectl.
    /// </summary>
    public const string BootstrapImage = "alpine/k8s:1.29.4";

    const string Component = "vault";
    const string ConfigVolume = "config";
    const string DataVolume = "data";
    const string DataPath = "/vault/data";

    /// <summary>
    /// Gets the settings of the Vault of a Production, as the Secret and key holding the token Chronicle authenticates with.
    /// </summary>
    /// <param name="spec">The spec of the Production.</param>
    /// <param name="names">The names of what the Production owns.</param>
    /// <returns>The Secret name and key, or null when the Production has no Vault.</returns>
    public static (string Secret, string Key)? ChronicleToken(ProductionSpec spec, ProductionNames names) => spec.Vault.Mode switch
    {
        ProductionModes.Managed => (names.VaultChronicleToken, TokenKey),
        ProductionModes.External when spec.Vault.TokenSecretRef is { } reference => (reference.Name, TokenKeyOf(reference)),
        _ => null
    };

    /// <summary>
    /// Gets the key a token is in, in the Secret a spec names it in.
    /// </summary>
    /// <param name="reference">The Secret reference.</param>
    /// <returns>The key named, or <see cref="TokenKey"/>.</returns>
    public static string TokenKeyOf(ProductionSecretReference reference) => string.IsNullOrEmpty(reference.Key) ? TokenKey : reference.Key;

    /// <summary>
    /// Gets the address of the Vault of a Production.
    /// </summary>
    /// <param name="spec">The spec of the Production.</param>
    /// <param name="names">The names of what the Production owns.</param>
    /// <returns>The address, or null when the Production has no Vault.</returns>
    public static string? AddressOf(ProductionSpec spec, ProductionNames names) => spec.Vault.Mode switch
    {
        ProductionModes.Managed => names.VaultAddress,
        ProductionModes.External => spec.Vault.Address,
        _ => null
    };

    /// <summary>
    /// Gets the mode and tier a spec asks for, in the form the status keeps it.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>The mode, followed by the tier of a managed Vault, as mode or mode/tier.</returns>
    public static string ModeOf(ProductionSpec spec) =>
        string.Equals(spec.Vault.Mode, ProductionModes.Managed, StringComparison.Ordinal) ? $"{spec.Vault.Mode}/{spec.Vault.Managed.Tier}" : spec.Vault.Mode;

    /// <summary>
    /// Creates the Vault configuration file.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The configuration, in HCL.</returns>
    public static string Configuration(Production production)
    {
        var names = ProductionNames.For(production);
        var managed = production.Spec.Vault.Managed;
        var lines = new List<string> { "ui = false", "disable_mlock = true", string.Empty };

        if (managed.IsProductionTier())
        {
            lines.Add("storage \"raft\" {");
            lines.Add($"  path = \"{DataPath}\"");
            for (var node = 0; node < managed.Nodes(); node++)
            {
                lines.Add("  retry_join {");
                lines.Add($"    leader_api_addr = \"http://{names.Vault}-{node}.{names.VaultNodes}.{names.Namespace}.svc.cluster.local:{Port}\"");
                lines.Add("  }");
            }

            lines.Add("}");
        }
        else
        {
            lines.AddRange(["storage \"file\" {", $"  path = \"{DataPath}\"", "}"]);
        }

        lines.AddRange(
        [
            string.Empty,
            "listener \"tcp\" {",
            $"  address = \"0.0.0.0:{Port}\"",
            $"  cluster_address = \"0.0.0.0:{ClusterPort}\"",
            "  tls_disable = 1",
            "}"
        ]);

        if (SealOf(managed.Unseal) is { } seal)
        {
            lines.Add(string.Empty);
            lines.Add(seal);
        }

        return string.Join('\n', lines) + "\n";
    }

    /// <summary>
    /// Creates the bootstrap script.
    /// </summary>
    /// <returns>The script.</returns>
    public static string BootstrapScript() => BootstrapScripts.Vault;

    /// <summary>
    /// Creates the ConfigMap holding the configuration and the bootstrap script.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The ConfigMap.</returns>
    public static V1ConfigMap ConfigMap(Production production) => new()
    {
        ApiVersion = "v1",
        Kind = "ConfigMap",
        Metadata = ProductionMetadata.Create(production, ProductionNames.For(production).VaultConfig, Component),
        Data = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ConfigFile] = Configuration(production),
            [BootstrapFile] = BootstrapScript()
        }
    };

    /// <summary>
    /// Creates the ServiceAccount of the bootstrap helper.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The ServiceAccount.</returns>
    public static V1ServiceAccount ServiceAccount(Production production) => new()
    {
        ApiVersion = "v1",
        Kind = "ServiceAccount",
        Metadata = ProductionMetadata.Create(production, ProductionNames.For(production).VaultBootstrap, Component)
    };

    /// <summary>
    /// Creates the Role of the bootstrap helper: it reads and creates Secrets in the namespace of the Production, and nothing else.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The Role.</returns>
    public static V1Role Role(Production production) => new()
    {
        ApiVersion = "rbac.authorization.k8s.io/v1",
        Kind = "Role",
        Metadata = ProductionMetadata.Create(production, ProductionNames.For(production).VaultBootstrap, Component),
        Rules = [new() { ApiGroups = [string.Empty], Resources = ["secrets"], Verbs = ["get", "create"] }]
    };

    /// <summary>
    /// Creates the RoleBinding of the bootstrap helper.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The RoleBinding.</returns>
    public static V1RoleBinding RoleBinding(Production production)
    {
        var names = ProductionNames.For(production);
        return new()
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = "RoleBinding",
            Metadata = ProductionMetadata.Create(production, names.VaultBootstrap, Component),
            RoleRef = new() { ApiGroup = "rbac.authorization.k8s.io", Kind = "Role", Name = names.VaultBootstrap },
            Subjects = [new() { Kind = "ServiceAccount", Name = names.VaultBootstrap, NamespaceProperty = names.Namespace }]
        };
    }

    /// <summary>
    /// Creates the Service Chronicle reaches Vault through.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The Service.</returns>
    public static V1Service Service(Production production) => new()
    {
        ApiVersion = "v1",
        Kind = "Service",
        Metadata = ProductionMetadata.Create(production, ProductionNames.For(production).Vault, Component),
        Spec = new()
        {
            Type = "ClusterIP",
            Selector = ProductionMetadata.Labels(production, Component),
            Ports = [new() { Name = "http", Protocol = "TCP", Port = Port, TargetPort = "http" }]
        }
    };

    /// <summary>
    /// Creates the headless Service the nodes find each other through, which also names them.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The Service.</returns>
    public static V1Service NodesService(Production production) => new()
    {
        ApiVersion = "v1",
        Kind = "Service",
        Metadata = ProductionMetadata.Create(production, ProductionNames.For(production).VaultNodes, Component),
        Spec = new()
        {
            ClusterIP = "None",
            PublishNotReadyAddresses = true,
            Selector = ProductionMetadata.Labels(production, Component),
            Ports =
            [
                new() { Name = "http", Protocol = "TCP", Port = Port, TargetPort = "http" },
                new() { Name = "cluster", Protocol = "TCP", Port = ClusterPort, TargetPort = "cluster" }
            ]
        }
    };

    /// <summary>
    /// Creates the StatefulSet of the Vault nodes.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The StatefulSet.</returns>
    public static V1StatefulSet StatefulSet(Production production)
    {
        var names = ProductionNames.For(production);
        var managed = production.Spec.Vault.Managed;
        var labels = ProductionMetadata.Labels(production, Component);
        var autoUnseal = !string.Equals(managed.Unseal.Mode, ProductionVaultUnseal.KubernetesSecret, StringComparison.Ordinal);

        var claim = new V1PersistentVolumeClaim
        {
            Metadata = new() { Name = DataVolume },
            Spec = new()
            {
                AccessModes = ["ReadWriteOnce"],
                Resources = new() { Requests = new Dictionary<string, ResourceQuantity>(StringComparer.Ordinal) { ["storage"] = new(managed.StorageSize) } }
            }
        };
        if (!string.IsNullOrWhiteSpace(managed.StorageClassName))
        {
            claim.Spec.StorageClassName = managed.StorageClassName;
        }

        var podName = new V1EnvVar { Name = "POD_NAME", ValueFrom = new() { FieldRef = new() { FieldPath = "metadata.name" } } };
        var vaultEnvironment = new List<V1EnvVar>
        {
            podName,
            new() { Name = "VAULT_ADDR", Value = $"http://127.0.0.1:{Port}" },
            new() { Name = "VAULT_API_ADDR", Value = $"http://$(POD_NAME).{names.VaultNodes}.{names.Namespace}.svc.cluster.local:{Port}" },
            new() { Name = "VAULT_CLUSTER_ADDR", Value = $"http://$(POD_NAME).{names.VaultNodes}.{names.Namespace}.svc.cluster.local:{ClusterPort}" },
            new() { Name = "VAULT_RAFT_NODE_ID", Value = "$(POD_NAME)" }
        };
        var vaultEnvironmentFrom = new List<V1EnvFromSource>();
        AddUnsealCredentials(managed.Unseal, vaultEnvironment, vaultEnvironmentFrom);

        var bootstrapEnvironment = new List<V1EnvVar>
        {
            podName,
            new() { Name = "HOME", Value = "/tmp" },
            new() { Name = "VAULT_ADDR", Value = $"http://127.0.0.1:{Port}" },
            new() { Name = "NAMESPACE", ValueFrom = new() { FieldRef = new() { FieldPath = "metadata.namespace" } } },
            new() { Name = "KEYS_SECRET", Value = names.VaultKeys },
            new() { Name = "CHRONICLE_SECRET", Value = names.VaultChronicleToken },
            new() { Name = "TIER", Value = managed.Tier },
            new() { Name = "UNSEAL", Value = autoUnseal ? "auto" : "secret" }
        };

        return new()
        {
            ApiVersion = "apps/v1",
            Kind = "StatefulSet",
            Metadata = ProductionMetadata.Create(production, names.Vault, Component),
            Spec = new()
            {
                Replicas = managed.Nodes(),
                ServiceName = names.VaultNodes,
                PodManagementPolicy = "Parallel",
                UpdateStrategy = new() { Type = "RollingUpdate" },
                Selector = new() { MatchLabels = labels },
                VolumeClaimTemplates = [claim],
                Template = new()
                {
                    Metadata = new()
                    {
                        Labels = labels,
                        Annotations = new Dictionary<string, string> { [ConfigRevisionAnnotation] = Revision(production) }
                    },
                    Spec = new()
                    {
                        ServiceAccountName = names.VaultBootstrap,
                        SecurityContext = new() { FsGroup = 1000, SeccompProfile = new() { Type = "RuntimeDefault" } },
                        Volumes = [new() { Name = ConfigVolume, ConfigMap = new() { Name = names.VaultConfig, DefaultMode = 0x1A4 } }],
                        Containers =
                        [
                            new()
                            {
                                Name = Component,
                                Image = managed.ImageOrDefault(),
                                Command = ["vault", "server", $"-config=/vault/config/{ConfigFile}"],
                                Ports =
                                [
                                    new() { Name = "http", ContainerPort = Port },
                                    new() { Name = "cluster", ContainerPort = ClusterPort }
                                ],
                                Env = vaultEnvironment,
                                EnvFrom = vaultEnvironmentFrom,
                                VolumeMounts =
                                [
                                    new() { Name = ConfigVolume, MountPath = "/vault/config", ReadOnlyProperty = true },
                                    new() { Name = DataVolume, MountPath = DataPath }
                                ],

                                // A node that is sealed or not initialized answers an error status and so is not ready; a standby answers success.
                                ReadinessProbe = new() { HttpGet = new() { Path = "/v1/sys/health?standbyok=true&perfstandbyok=true", Port = "http" }, PeriodSeconds = 5 },
                                SecurityContext = new() { RunAsUser = 100, RunAsGroup = 1000, AllowPrivilegeEscalation = false, Capabilities = new() { Drop = ["ALL"] } }
                            },
                            new()
                            {
                                Name = "bootstrap",
                                Image = BootstrapImage,
                                Command = ["/bin/bash", $"/vault/bootstrap/{BootstrapFile}"],
                                Env = bootstrapEnvironment,
                                VolumeMounts = [new() { Name = ConfigVolume, MountPath = "/vault/bootstrap", ReadOnlyProperty = true }],
                                SecurityContext = new() { RunAsNonRoot = true, RunAsUser = 1000, RunAsGroup = 1000, AllowPrivilegeEscalation = false, Capabilities = new() { Drop = ["ALL"] } }
                            }
                        ]
                    }
                }
            }
        };
    }

    static string? SealOf(ProductionVaultUnseal unseal)
    {
        switch (unseal.Mode)
        {
            case ProductionVaultUnseal.CloudKeyService:
                var settings = unseal.Cloud.Settings;
                return unseal.Cloud.Provider switch
                {
                    VaultCloudProviders.Aws => Seal("awskms", [("region", Setting(settings, "region")), ("kms_key_id", Setting(settings, "kmsKeyId"))]),
                    VaultCloudProviders.Azure => Seal(
                        "azurekeyvault",
                        [("tenant_id", Setting(settings, "tenantId")), ("vault_name", Setting(settings, "vaultName")), ("key_name", Setting(settings, "keyName"))]),
                    VaultCloudProviders.Google => Seal(
                        "gcpckms",
                        [("project", Setting(settings, "project")), ("region", Setting(settings, "region")), ("key_ring", Setting(settings, "keyRing")), ("crypto_key", Setting(settings, "cryptoKey"))]),
                    _ => null
                };

            case ProductionVaultUnseal.TransitVault:
                return Seal(
                    "transit",
                    [("address", unseal.Transit.Address), ("key_name", unseal.Transit.KeyName), ("mount_path", unseal.Transit.MountPath), ("disable_renewal", "false")]);

            default:
                return null;
        }
    }

    static string Setting(IDictionary<string, string> settings, string name) => settings.TryGetValue(name, out var value) ? value : string.Empty;

    static string Seal(string type, (string Name, string Value)[] attributes) =>
        $"seal \"{type}\" {{\n{string.Concat(attributes.Where(attribute => !string.IsNullOrEmpty(attribute.Value)).Select(attribute => $"  {attribute.Name} = \"{attribute.Value}\"\n"))}}}\n";

    static void AddUnsealCredentials(ProductionVaultUnseal unseal, List<V1EnvVar> environment, List<V1EnvFromSource> environmentFrom)
    {
        if (string.Equals(unseal.Mode, ProductionVaultUnseal.CloudKeyService, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(unseal.Cloud.CredentialsSecretName))
        {
            environmentFrom.Add(new() { SecretRef = new() { Name = unseal.Cloud.CredentialsSecretName } });
        }

        if (string.Equals(unseal.Mode, ProductionVaultUnseal.TransitVault, StringComparison.Ordinal) && unseal.Transit.TokenSecretRef is { } token)
        {
            environment.Add(new() { Name = "VAULT_TOKEN", ValueFrom = new() { SecretKeyRef = new() { Name = token.Name, Key = TokenKeyOf(token) } } });
        }
    }

    static string Revision(Production production)
    {
        var data = ConfigMap(production).Data;
        var content = string.Concat(data.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => $"{pair.Key}\n{pair.Value}\n"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)))[..16].ToLowerInvariant();
    }
}
