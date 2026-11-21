---
title: Declare Stage Manager resources with Pulumi
description: Declare a Stage Manager Production, Performance and Understudy from a C# Pulumi program with typed components, and let them write the Kubernetes Secrets the resources name.
---

The `Cratis.Showrunner.StageManager.Pulumi` package is for a cluster that a C# [Pulumi](https://www.pulumi.com/) program already deploys. It has one component for each resource you would otherwise write as YAML: `Production`, `Performance` and `Understudy`. You pass typed arguments, including the outputs of other resources, and the component creates the custom resource and the Kubernetes Secrets it names. There are no templates and no tokens to fill.

The package does not install the operator or its custom resource definitions. [Install the operator](./install.md) first; its Helm chart installs the definitions, and the namespace of every resource below must be one of the chart's `managedNamespaces`.

## Install the package

```shell
dotnet add package Cratis.Showrunner.StageManager.Pulumi
```

It supports `net10.0` and `net11.0` and depends on `Pulumi` and `Pulumi.Kubernetes`. It does not reference the operator, so it does not bring the operator's code into your program.

The components live in three namespaces:

```csharp
using Cratis.Showrunner.StageManager.Pulumi;               // SecretReferenceArgs
using Cratis.Showrunner.StageManager.Pulumi.Productions;   // Production
using Cratis.Showrunner.StageManager.Pulumi.Performances;  // Performance
using Cratis.Showrunner.StageManager.Pulumi.Understudies;  // Understudy
```

## How the components behave

- Each component is a Pulumi `ComponentResource` with the type token `cratis:showrunner:Production`, `cratis:showrunner:Performance` or `cratis:showrunner:Understudy`. It takes the Pulumi name, its args and an optional `ComponentResourceOptions`.
- Pass the Kubernetes provider in the args (`Provider`). When it is null, the default provider is used.
- The Kubernetes name of the resource is `Name`, or the Pulumi name when `Name` is null. It is exposed as `KubernetesName`.
- Optional settings are nullable. A setting you leave out is not written, so the operator's own default applies. Required settings are `required` properties.
- Settings that refer to a ConfigMap work three ways: name one that exists, give the files as text, or give a directory of files.
- Secrets work two ways. Name a Secret that already exists, or give the credential as a value (a string or an `Output<string>`) and the component creates the Secret next to the resource. Created Secrets hold Pulumi secrets, so the value is encrypted in the stack state and masked in the output.
- The custom resource depends on the Secrets the component creates and on everything in `DependsOn`, so the operator never reconciles a half-made set.
- `Labels` go on the custom resource and on every Secret or ConfigMap the component creates.

## Declare a Production

A `Production` is Chronicle with its database, Vault and optional graph. This example adopts an existing MongoDB and Vault and dials Cratis Direct. Pulumi keeps the database and Vault, and the operator runs Chronicle.

```csharp
var production = new Production(
    "chronicle",
    new ProductionArgs
    {
        Provider = provider,
        Name = "keldakraft",
        Namespace = "keldakraft",
        Version = "19.164.2",
        Replicas = 2,
        Database = new ProductionDatabaseArgs
        {
            ConnectionSecretName = "keldakraft-mongo",
            ConnectionString = mongoUri,
        },
        Vault = new ProductionVaultArgs
        {
            Mode = VaultMode.External,
            Address = vaultAddress,
            Token = vaultToken,
        },
        Federation = new ProductionFederationArgs
        {
            Dial = new FederationDialArgs
            {
                EventStore = "KeldaKraft",
                Serve = true,
                EnrollmentCode = enrollmentCode,
            },
        },
        DependsOn = { applicationNamespace },
    });
```

With these arguments the component creates:

| Secret | Key | Holds |
| --- | --- | --- |
| `keldakraft-mongo` | `connectionString` | `Database.ConnectionString` |
| `keldakraft-vault-token` | `token` | `Vault.Token` |
| `keldakraft-enrollment-code` | `code` | `Federation.Dial.EnrollmentCode` |

Name them yourself with `Database.ConnectionSecretName`, `Vault.TokenSecretName` and `Vault.TokenSecretKey`, and `Dial.EnrollmentCodeSecretName` and `Dial.EnrollmentCodeSecretKey`. When you name a Secret and give no value, the component writes nothing and the custom resource refers to the Secret you named.

`ProductionArgs` covers the whole `ProductionSpec` of the operator:

| Property | Type | Notes |
| --- | --- | --- |
| `Namespace`, `Version` | `Input<string>` | Required. `Version` is the tag of the `cratis/chronicle` image |
| `Replicas` | `int` | Defaults to 1 |
| `Database` | `ProductionDatabaseArgs` | Required. `Kind` (`MongoDB`, `PostgreSql`, `SqlServer`) and `Mode` (`External` by default, or `Managed`) are always written |
| `Vault` | `ProductionVaultArgs` | `Mode` is `None`, `Managed` or `External`. A managed Vault takes `ManagedVaultArgs`, with a `VaultTier`, storage and `VaultUnsealArgs` for the Kubernetes Secret, cloud key service or transit Vault |
| `Federation` | `ProductionFederationArgs` | The listener (`Enabled`, `Port`), the hosted authority (`Hosted`) and dialing out (`Dial`). `Dial.Enabled` and `Hosted.Enabled` default to true, because naming them means wanting them |
| `Graph` | `ProductionGraphArgs` | `Mode` defaults to `Managed`. An external graph names its credentials Secret |
| `Sinks` | `ProductionSinksArgs` | Named sinks, each with a `SinkKind` and a connection Secret or graph |

### Let the operator run the database and back it up

A managed database can attach object storage for Percona Backup for MongoDB. The component writes the credentials Secret with the keys Percona reads, `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY`, and the operator attaches the storage with the PBM `minio` type.

```csharp
Database = new ProductionDatabaseArgs
{
    Mode = DatabaseMode.Managed,
    Members = 3,
    MemoryLimit = "8Gi",
    StorageSize = "20Gi",
    NodeSelector = { ["workload"] = "data" },
    Backup = new DatabaseBackupArgs
    {
        Storage = new DatabaseBackupStorageArgs
        {
            Bucket = "keldakraft-backups",
            Endpoint = "objects.example.com",
            Region = "europe-1",
            AccessKeyId = accessKeyId,
            SecretAccessKey = secretAccessKey,
        },
    },
},
```

Leave `Tasks` empty for the operator's schedules (hourly 24, daily 7, weekly 4, monthly 12), or list `BackupTaskArgs`. The credentials Secret is `<name>-backup-credentials` unless `CredentialsSecretName` says otherwise. Give both `AccessKeyId` and `SecretAccessKey`, or neither.

## Declare a Performance

A `Performance` runs an application against a Production. An application with a worker next to its service declares one `Performance` for each component, all against the same Production.

```csharp
var service = new Performance(
    "keldakraft",
    new PerformanceArgs
    {
        Provider = provider,
        Namespace = "keldakraft",
        ProductionName = production.KubernetesName,
        Application = new ApplicationArgs
        {
            Image = serviceImage,
            EventStores = ["KeldaKraft"],
            Port = 8080,
            Strategy = RolloutStrategy.Recreate,
            ImagePullSecrets = ["registry-credentials"],
            Probes = new ApplicationProbesArgs
            {
                StartupPath = "/health",
                LivenessPath = "/health",
                ReadinessPath = "/health",
            },
            Resources = new ApplicationResourcesArgs
            {
                Requests = { ["cpu"] = "250m", ["memory"] = "512Mi" },
                Limits = { ["cpu"] = "1", ["memory"] = "1Gi" },
            },
            Egress = [new EgressRuleArgs { Ports = [587, 80, 7448, 443] }],
        },
        Environment = new PerformanceEnvironmentArgs
        {
            Name = "production",
            ConfigMapName = "keldakraft-environment",
            Content = environmentFile,
        },
    });

var ingest = new Performance(
    "keldakraft-ingest",
    new PerformanceArgs
    {
        Provider = provider,
        Namespace = "keldakraft",
        ProductionName = production.KubernetesName,
        Application = new ApplicationArgs
        {
            Image = ingestImage,
            EventStores = ["KeldaKraft"],
            ImagePullSecrets = ["registry-credentials"],
            Egress = [new EgressRuleArgs { Ports = [7448, 443] }],
        },
        Environment = new PerformanceEnvironmentArgs
        {
            Name = "production",
            ConfigMapName = "keldakraft-environment",
        },
        DependsOn = { service },
    });
```

The `Environment` carries the [Cratis environment file](./environments-and-secrets.md) the operator resolves. Give it as text in `Content` and the component writes it to the ConfigMap `ConfigMapName`, under the key `<environment name>.yaml` unless `ConfigMapKey` says otherwise. Only one `Performance` of an application writes the ConfigMap: the others name it and depend on that one, as `ingest` does above. The environment file holds plain values, encrypted values and references, never plain secrets.

Name the Chronicle with `ProductionName` (and `ProductionNamespace` when the Production is in another namespace) or with `ChronicleConnectionSecret`, an existing Secret that holds a connection string. The component throws an `ArgumentException` when you give neither.

`ApplicationArgs` covers the application part of the operator's `PerformanceSpec`: `Image`, `EventStores`, `Port`, `Replicas`, `Sinks`, `Graph`, `Strategy`, `ImagePullSecrets`, `Probes`, `Resources`, `NodeSelector`, `Volumes`, `VolumeMounts`, `InitContainers` and `Egress`. `PerformanceArgs` adds `Lifetime` for a Performance that expires. Naming any `Egress` rule isolates the pods for egress, as described in [Run an application with a Performance](./performance.md).

The component does not cover a published Stage application. Declare that in the YAML of the [Performance](./performance.md) until the package supports it.

### AuthProxy, Ante, the Ingress and the signing keys

AuthProxy, Ante and the Ingress are optional, in Pulumi as in YAML: declare `AuthProxy`, `Onboarding` and `Ingress` to run them, leave one out and nothing is written for it. [Run an application with a Performance](./performance.md#authproxy-and-ante-are-optional) says what each omission means. Every field of those blocks is nullable; null is not written, so the operator's default applies.

```csharp
var keldakraft = new Performance(
    "keldakraft",
    new PerformanceArgs
    {
        Provider = provider,
        Namespace = "keldakraft",
        ProductionName = production.KubernetesName,
        Application = new ApplicationArgs { Image = serviceImage, EventStores = ["KeldaKraft"] },
        Environment = new PerformanceEnvironmentArgs
        {
            Name = "production",
            ConfigMapName = "keldakraft-environment",
            Content = environmentFile,           // holds ante.mongodb.server
        },
        AuthProxy = new AuthProxyArgs
        {
            Providers =
            [
                new AuthProxyProviderArgs
                {
                    Kind = AuthProxyProviderKind.GitHub,
                    Identity = "GitHub",
                    ClientId = config.RequireSecret("gitHubClientId"),
                    ClientSecret = config.RequireSecret("gitHubClientSecret"),
                    Scopes = ["read:user", "user:email", "read:org"],
                },
            ],
            Tenancy = new AuthProxyTenancyArgs { TenantsEndpoint = "http://keldakraft-app:8080/api/internal/tenants/selectable" },
            RequiredClaims =
            [
                new RequiredClaimArgs { Claim = "urn:github:organization", AnyOf = ["kelda"] },
                new RequiredClaimArgs { Claim = "urn:github:team", AnyOf = ["kelda/staff"] },
            ],
            Pages = new AuthProxyPagesArgs { Directory = "AuthProxyPages" },
            DataProtection = new AuthProxyDataProtectionArgs { Size = "1Gi" },
            Resources = new ApplicationResourcesArgs { Requests = { ["cpu"] = "100m", ["memory"] = "128Mi" } },
        },
        Onboarding = new OnboardingArgs
        {
            SignUp = SignUpMode.JoinTenant,
            Personas = ["Administrator", "Member"],
            AllowedLocales = ["en"],
            ReservedOrganizationNames = ["admin", "api", "app"],
            Branding = new AnteBrandingArgs
            {
                LogoUrl = "/brand/logo.svg",
                CustomCssUrl = "/brand/lobby.css",
                PageTitle = "Kelda Kraft",
                SplashMessage = "Loading Kelda Kraft",
                Assets = new AnteBrandingAssetsArgs { Directory = "Lobby/Branding" },
            },
        },
        Ingress = new IngressArgs
        {
            ClassName = "nginx",
            Hosts = ["app.keldakraft.example.com"],
            TlsIssuerName = "letsencrypt-prod",
            Annotations =
            {
                ["nginx.ingress.kubernetes.io/proxy-read-timeout"] = "3600",
                ["nginx.ingress.kubernetes.io/proxy-send-timeout"] = "3600",
            },
        },
    });
```

The component writes what the settings refer to when you give values instead of names:

| You give | The component writes |
| --- | --- |
| `ClientId` and `ClientSecret` of a provider | The Secret `<name>-<identity>-credentials` with the keys `clientId` and `clientSecret`, as Pulumi secrets. The provider's `credentialsSecretRef` names it. Or give `CredentialsSecretName` (and `ClientIdKey`, `ClientSecretKey`) for a Secret that exists, or neither to take them from the Cratis environment |
| `Pages` as `Files` or a `Directory` | The ConfigMap `<name>-authproxy-pages`. Or give `ConfigMapName` for one that exists. Give exactly one of the three |
| `DataProtection` without `ClaimName` | A `ReadWriteOnce` PersistentVolumeClaim `<name>-authproxy-keys` of `Size` (default `1Gi`) and the optional `StorageClassName`. Or give `ClaimName` for one that exists |
| `Legal` as `TermsAndConditions` and `PrivacyPolicy` | The ConfigMap `<name>-legal` with `terms.md` and `privacy.md`. Or give `ConfigMapName` |
| `Branding.Assets` as `Files` or a `Directory` | The ConfigMap `<name>-brand`, and the file names to serve. Or give `ConfigMapName` and `FileNames` for one that exists |

A directory is read when the program runs, from the files directly in it, so run the program from the folder a relative path is relative to or give an absolute one. The files are text: a ConfigMap holds no binary content, so a logo is an SVG or lives on another host.

The Performance depends on everything the component writes, so the operator never reads a Secret that is not there yet. `SigningKeysArgs` (`Rotation`, `RetainPrevious`) sets the [signing keys](./performance.md#signing-keys).

#### A gateway in front of an existing Service

Give the `AuthProxy` an `Upstream` and the Performance is a [gateway](./performance.md#run-only-authproxy-in-front-of-a-service): `Application`, `Environment`, `ProductionName` and `ChronicleConnectionSecret` are all left out. The component throws an `ArgumentException` when a gateway without an environment has a provider that gets neither `ClientId` and `ClientSecret` nor a `CredentialsSecretName`.

```csharp
var operations = new Performance(
    "keldakraft-operations",
    new PerformanceArgs
    {
        Provider = provider,
        Namespace = "keldakraft",
        AuthProxy = new AuthProxyArgs
        {
            Upstream = new AuthProxyUpstreamArgs { Url = "http://grafana.infrastructure.svc.cluster.local:3000/" },
            Providers =
            [
                new AuthProxyProviderArgs
                {
                    Kind = AuthProxyProviderKind.GitHub,
                    Identity = "GitHub",
                    ClientId = config.RequireSecret("operationsGitHubClientId"),
                    ClientSecret = config.RequireSecret("operationsGitHubClientSecret"),
                    Scopes = ["read:user", "user:email", "read:org"],
                },
            ],
            Tenancy = new AuthProxyTenancyArgs { TenantId = "Default" },
            RequiredClaims =
            [
                new RequiredClaimArgs { Claim = "urn:github:organization", AnyOf = ["kelda"] },
                new RequiredClaimArgs { Claim = "urn:github:team", AnyOf = ["kelda/staff"] },
            ],
        },
        Ingress = new IngressArgs { ClassName = "nginx", Hosts = ["operations.keldakraft.example.com"], TlsIssuerName = "letsencrypt-prod" },
    });
```

Declare one gateway for each console, each with its own host and its own GitHub OAuth app.

## Declare an Understudy

An `Understudy` backs up a Production's databases and, optionally, its Vault keys to object storage. The operator refuses a key backup whose bucket is the data bucket, so the keys get a bucket and credentials of their own.

```csharp
var understudy = new Understudy(
    "backups",
    new UnderstudyArgs
    {
        Provider = provider,
        Name = "keldakraft",
        Namespace = "keldakraft",
        ProductionName = production.KubernetesName,
        Schedule = "0 * * * *",
        MaxAgeDays = 35,
        Retention = new UnderstudyRetentionArgs { Hourly = 24, Daily = 7, Weekly = 4 },
        Storage = new UnderstudyStorageArgs
        {
            Endpoint = endpoint,
            Region = region,
            Bucket = backupBucket,
            PathStyle = true,
            AccessKey = backupAccessKey,
            SecretKey = backupSecretKey,
        },
        Keys = new UnderstudyKeysArgs
        {
            Schedule = "0 2 * * *",
            Storage = new UnderstudyStorageArgs
            {
                Endpoint = endpoint,
                Region = region,
                Bucket = keysBucket,
                PathStyle = true,
                AccessKey = keysAccessKey,
                SecretKey = keysSecretKey,
            },
            Certificate = keyBackupCertificate,
            VaultTokenSecret = new SecretReferenceArgs { Name = "keldakraft-vault-token", Key = "token" },
        },
    });
```

The component creates these Secrets, with the keys the operator reads:

| Secret | Keys | Holds |
| --- | --- | --- |
| `<name>-backup-credentials` | `accessKey`, `secretKey` (S3) or `accountName`, `accountKey` (Azure Blob) | `Storage` credentials |
| `<name>-key-backup-credentials` | the same | `Keys.Storage` credentials |
| `<name>-key-backup-certificate` | `certificate.pem` | `Keys.Certificate` |

Pass only the public certificate in `Keys.Certificate`: keep its private key out of the cluster, with whoever may restore the Vault keys. Name existing Secrets with `CredentialsSecretName` on the storage and `CertificateSecretName` on the keys instead of giving values. `Keys.VaultTokenSecret` always names an existing Secret, such as the token Secret a `Production` writes for its external Vault.

`Provider` on `UnderstudyStorageArgs` is `S3` by default, or `AzureBlob` or `Volume`. Use `AccountName` and `AccountKey` for Azure Blob.

## How the package stays in step with the operator

The package never references the operator, so the two could drift. Its specifications guard against that. Each one runs a realistic program against Pulumi's test mocks, serializes the custom resource the component emitted, and reads it back with the operator's own `ProductionSpec`, `PerformanceSpec` or `UnderstudySpec`. Reading is strict: a property the operator does not have, or one spelled with another case, fails. The specifications then run the operator's `ProductionValidator`, `SpecValidator` or `UnderstudyValidator` over the result, so an invalid combination fails too.

## Troubleshooting

| Symptom | Cause | Fix |
| --- | --- | --- |
| `no matches for kind "Production"` | The operator's custom resource definitions are not installed | [Install the operator](./install.md) before running `pulumi up` |
| The custom resource is created but never reconciles | The namespace is not one of the chart's `managedNamespaces` | Add it to the chart values |
| The Production reports a problem naming a field | The operator's validator rejected the spec | Read `status` of the resource; the message names the field, and the same rule runs in this package's specifications |
| `ArgumentException` while previewing | A component was given both halves of a credential pair incompletely, or no Chronicle | The message names the argument to add |
