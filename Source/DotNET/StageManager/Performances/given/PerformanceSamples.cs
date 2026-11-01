// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.given;

/// <summary>
/// Performances and environments the specifications run on. Every secret value is a recognizable fake.
/// </summary>
public static class PerformanceSamples
{
    public const string EntraClientSecret = "fake-entra-client-secret-7d1f";
    public const string GitHubClientSecret = "fake-github-client-secret-33aa";
    public const string MongoServer = "mongodb://fake-user:fake-password-91c@mongo.internal:27017";
    public const string ConnectionString = "chronicle://fake-client:fake-chronicle-secret-5e2@chronicle:35000";

    public const string StorageConnectionString = "mongodb://fake-stage:fake-storage-password-4b8@mongo.internal:27017/crm";
    public const string NotesToken = "fake-notes-collector-token-6c0";

    public static IReadOnlyList<string> SecretValues => [EntraClientSecret, GitHubClientSecret, MongoServer, ConnectionString, StorageConnectionString, NotesToken];

    public static Performance PublishedPerformance(string name = "crm", bool notes = true)
    {
        var performance = Performance(name, onboarding: false);
        performance.Spec.Chronicle = null;
        performance.Spec.Application.Image = "cratis/stage:5.3.0";
        performance.Spec.Application.Published = new()
        {
            Version = "1.4.0",
            PackageConfigMapRef = new() { Name = $"{name}-package-1-4-0" },
            StorageConnectionSecretRef = new() { Name = $"{name}-storage" },
            TrustedProxies = ["10.244.0.0/16"],
            Notes = notes
                ? new()
                {
                    CollectorUrl = "https://collector.example.invalid:8443",
                    CollectorTokenSecretRef = new() { Name = $"{name}-notes-token" },
                    PublicationId = "pub-7c1d",
                    CollectorTenantId = "acme-corp",
                    ApplicationName = "Crm"
                }
                : null
        };
        return performance;
    }

    public static Performance Performance(string name = "crm", bool onboarding = true, bool authentication = true)
    {
        return new Performance
        {
            Metadata = new()
            {
                Name = name,
                NamespaceProperty = "apps",
                Uid = $"uid-{name}",
                Generation = 3
            },
            Spec = new()
            {
                Chronicle = new() { ConnectionSecretRef = new() { Name = "chronicle-connection" } },
                Application = new()
                {
                    Image = "registry.example.com/crm:1.0.0",
                    EventStores = ["Crm"]
                },
                Environment = new() { Name = "production", ConfigMapRef = new() { Name = "crm-environment" } },
                AuthProxy = new()
                {
                    Enabled = authentication,
                    Providers =
                    [
                        new() { Kind = AuthProxyProviderKind.EntraId, Identity = "EntraId" },
                        new() { Kind = AuthProxyProviderKind.GitHub, Identity = "GitHub" }
                    ]
                },
                Onboarding = new()
                {
                    Enabled = onboarding,
                    InvitationTypes = ["JoinTenant"],
                    Personas = ["Administrator", "Member"],
                    Legal = new() { ConfigMapRef = new() { Name = "crm-legal" } },
                    AttestedProviders = [new() { Key = "entraid", Issuer = "https://login.example.com/tenant/v2.0", AcceptableAssurances = ["mfa"] }]
                },
                Ingress = new()
                {
                    Enabled = true,
                    ClassName = "nginx",
                    Hosts = [$"{name}.example.com"],
                    TlsIssuer = new() { Name = "letsencrypt" }
                }
            }
        };
    }

    public static ResolvedEnvironment Environment() => new("production", new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["authentication.providers.EntraId.tenantId"] = "fake-tenant-id",
        ["authentication.providers.EntraId.clientId"] = "fake-entra-client-id",
        ["authentication.providers.EntraId.clientSecret"] = EntraClientSecret,
        ["authentication.providers.GitHub.clientId"] = "fake-github-client-id",
        ["authentication.providers.GitHub.clientSecret"] = GitHubClientSecret,
        ["authentication.providers.Unused.clientSecret"] = "fake-unused-secret",
        ["ante.mongodb.server"] = MongoServer,
        ["smtp.host"] = "smtp.example.com"
    });

    public static SigningKeyState Keys() => SigningKeys.Ensure(null, 0, 2);
}
#endif
