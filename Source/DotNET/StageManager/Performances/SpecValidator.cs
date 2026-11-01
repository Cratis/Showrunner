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

        if (spec.ProductionRef is null && spec.Chronicle is null)
        {
            problems.Add("Name the Chronicle to use with productionRef or chronicle.connectionSecretRef.");
        }

        ValidateProviders(spec, problems);
        ValidateOnboarding(spec, problems);

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
}
