// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Finds what is wrong with a <see cref="Performance"/> before anything is resolved or applied.
/// </summary>
public static partial class SpecValidator
{
    [System.Text.RegularExpressions.GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9 _-]*$", System.Text.RegularExpressions.RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial System.Text.RegularExpressions.Regex ProviderIdentity { get; }

    /// <summary>
    /// Validates a Performance.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <returns>The problems, empty when the spec is valid.</returns>
    public static IReadOnlyList<string> Validate(Performance performance)
    {
        var spec = performance.Spec;
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(spec.Application.Image))
        {
            problems.Add("application.image is required.");
        }

        if (string.IsNullOrWhiteSpace(spec.Environment.Name))
        {
            problems.Add("environment.name is required.");
        }

        if (string.IsNullOrWhiteSpace(spec.Environment.ConfigMapRef.Name))
        {
            problems.Add("environment.configMapRef.name is required.");
        }

        if (spec.ProductionRef is null && spec.Chronicle is null && spec.Application.Published is null)
        {
            problems.Add("Name the Chronicle to use with productionRef or chronicle.connectionSecretRef.");
        }

        if (spec.Application.Sinks.Any(string.IsNullOrWhiteSpace))
        {
            problems.Add("application.sinks may not hold an empty name.");
        }

        if (spec.Application.Sinks.Count > 0 && spec.Application.Published is not null)
        {
            problems.Add("application.sinks is for a traditional application: a published Stage package declares its sinks in its manifest.");
        }
        else if (spec.Application.Sinks.Count > 0 && spec.ProductionRef is null)
        {
            problems.Add("application.sinks needs productionRef: the Production maps each sink to its storage.");
        }

        if (spec.Application.Graph && spec.Application.Published is not null)
        {
            problems.Add("application.graph is for a traditional application on a Production.");
        }
        else if (spec.Application.Graph && spec.ProductionRef is null)
        {
            problems.Add("application.graph needs productionRef: the graph is the Production's.");
        }

        ValidateProviders(spec, problems);
        ValidateOnboarding(spec, problems);
        ValidatePublished(spec, problems);
        ValidateLifetime(spec, problems);

        if (spec.Ingress.Enabled && spec.Ingress.Hosts.Count == 0)
        {
            problems.Add("ingress.hosts needs at least one host when the ingress is enabled.");
        }

        if (spec.Ingress.Enabled && !spec.AuthProxy.Enabled)
        {
            problems.Add("ingress needs authProxy.enabled: AuthProxy is the only public entry point.");
        }

        return problems;
    }

    static void ValidateProviders(PerformanceSpec spec, List<string> problems)
    {
        if (!spec.AuthProxy.Enabled)
        {
            return;
        }

        if (spec.AuthProxy.Providers.Count == 0)
        {
            problems.Add("authProxy.providers needs at least one provider when AuthProxy is enabled.");
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var provider in spec.AuthProxy.Providers)
        {
            if (!ProviderIdentity.IsMatch(provider.Identity))
            {
                problems.Add($"The provider identity '{provider.Identity}' may only hold letters, digits, spaces, hyphens and underscores.");
            }
            else if (!seen.Add(provider.Identity))
            {
                problems.Add($"The provider identity '{provider.Identity}' is used twice.");
            }
        }
    }

    static void ValidateOnboarding(PerformanceSpec spec, List<string> problems)
    {
        if (!spec.Onboarding.Enabled)
        {
            return;
        }

        if (!spec.AuthProxy.Enabled)
        {
            problems.Add("onboarding needs authProxy.enabled: invitations are exchanged through AuthProxy.");
        }

        if (!spec.Ingress.Enabled || spec.Ingress.Hosts.Count == 0)
        {
            problems.Add("onboarding needs ingress.hosts: Ante and AuthProxy are told the public URL.");
        }

        if (spec.Application.EventStores.Count == 0)
        {
            problems.Add("onboarding needs application.eventStores: the first one is the store Ante observes.");
        }

        var identity = spec.Onboarding.EventStore;
        if (identity is not null && spec.Application.EventStores.Contains(identity, StringComparer.Ordinal))
        {
            problems.Add("onboarding.eventStore must differ from the application's event stores: Ante keeps its own store.");
        }
    }

    static void ValidateLifetime(PerformanceSpec spec, List<string> problems)
    {
        if (spec.Lifetime is not { } lifetime)
        {
            return;
        }

        if (lifetime.Permanent == lifetime.Until.HasValue)
        {
            problems.Add("lifetime needs exactly one of permanent: true and until.");
        }

        if (lifetime.DataRetentionDays < 0)
        {
            problems.Add("lifetime.dataRetentionDays cannot be negative.");
        }
    }

    static void ValidatePublished(PerformanceSpec spec, List<string> problems)
    {
        if (spec.Application.Published is not { } published)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(published.Version))
        {
            problems.Add("application.published.version is required.");
        }

        if (string.IsNullOrWhiteSpace(published.PackageConfigMapRef.Name))
        {
            problems.Add("application.published.packageConfigMapRef.name is required.");
        }

        if (string.IsNullOrWhiteSpace(published.StorageConnectionSecretRef.Name))
        {
            problems.Add("application.published.storageConnectionSecretRef.name is required.");
        }

        if (!spec.AuthProxy.Enabled)
        {
            problems.Add("a published application needs authProxy.enabled: it requires a signed-in caller and trusts only AuthProxy.");
        }

        if (!spec.Ingress.Enabled || spec.Ingress.Hosts.Count == 0)
        {
            problems.Add("a published application needs ingress.hosts: Stage only answers the hosts it is published under.");
        }

        if (spec.Onboarding.Enabled)
        {
            problems.Add("onboarding is not supported for a published application.");
        }

        if (spec.Application.Replicas != 1)
        {
            problems.Add("a published application runs one replica: its kernel owns its storage.");
        }

        if (published.RolloutTimeoutSeconds <= 0)
        {
            problems.Add("application.published.rolloutTimeoutSeconds must be positive.");
        }

        if (published.TrustedProxies.Count == 0)
        {
            problems.Add("application.published.trustedProxies needs at least one address or network.");
        }

        foreach (var proxy in published.TrustedProxies.Where(proxy => !System.Net.IPAddress.TryParse(proxy, out _) && !System.Net.IPNetwork.TryParse(proxy, out _)))
        {
            problems.Add($"The trusted proxy '{proxy}' is neither an IP address nor a CIDR network.");
        }

        ValidateNotes(published.Notes, problems);
    }

    static void ValidateNotes(PublishedNotesSpec? notes, List<string> problems)
    {
        if (notes is not { Enabled: true })
        {
            return;
        }

        if (!Uri.TryCreate(notes.CollectorUrl, UriKind.Absolute, out var url) || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
        {
            problems.Add("application.published.notes.collectorUrl must be an absolute http or https URL.");
        }

        if (string.IsNullOrWhiteSpace(notes.CollectorTokenSecretRef.Name))
        {
            problems.Add("application.published.notes.collectorTokenSecretRef.name is required.");
        }

        if (string.IsNullOrWhiteSpace(notes.PublicationId))
        {
            problems.Add("application.published.notes.publicationId is required.");
        }

        if (string.IsNullOrWhiteSpace(notes.ApplicationName))
        {
            problems.Add("application.published.notes.applicationName is required.");
        }
    }
}
