// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>Decides whether a resource may run. The CRD schema checks the same rules; this is the guard when it did not.</summary>
public static partial class SpecValidator
{
    /// <summary>The reason given when the authentication is missing.</summary>
    public const string AuthenticationMissing = "AuthenticationMissing";

    /// <summary>The reason given for every other problem with the spec.</summary>
    public const string InvalidSpec = "InvalidSpec";

    [GeneratedRegex("^[a-z0-9]([-a-z0-9]{0,61}[a-z0-9])?$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DnsLabel { get; }

    [GeneratedRegex("^[a-z0-9]([-a-z0-9.]{0,251}[a-z0-9])?$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DnsSubdomain { get; }

    /// <summary>Finds everything that is wrong with a resource.</summary>
    /// <param name="application">The application.</param>
    /// <returns>The problems; empty when the resource may run.</returns>
    public static IReadOnlyList<SpecProblem> Validate(StageApplication application)
    {
        var spec = application.Spec;
        List<SpecProblem> problems = [];

        var authentication = spec.Authentication;
        if (authentication is null || string.IsNullOrWhiteSpace(authentication.ProxyImage) || string.IsNullOrWhiteSpace(authentication.ConfigSecretRef))
        {
            problems.Add(new(AuthenticationMissing, "spec.authentication with a proxyImage and a configSecretRef is required; an application is never run without sign-in."));
        }
        else if (authentication.Port is < 1 or > 65535 or ApplicationNames.StagePort or ApplicationNames.ChroniclePort or ApplicationNames.MongoPort)
        {
            problems.Add(new(InvalidSpec, $"spec.authentication.port {authentication.Port} is not a usable proxy port; it must be 1-65535 and not the Stage, Chronicle or MongoDB port."));
        }

        if (!DnsLabel.IsMatch(spec.PublishedName))
        {
            problems.Add(new(InvalidSpec, "spec.publishedName must be a DNS label (lowercase letters, digits and '-', at most 63 characters)."));
        }
        else if (!string.Equals(spec.PublishedName, application.Name, StringComparison.Ordinal))
        {
            problems.Add(new(InvalidSpec, "metadata.name must equal spec.publishedName."));
        }

        if (string.IsNullOrWhiteSpace(spec.Image))
        {
            problems.Add(new(InvalidSpec, "spec.image is required."));
        }

        if (string.IsNullOrWhiteSpace(spec.Version))
        {
            problems.Add(new(InvalidSpec, "spec.version is required."));
        }

        if (spec.Package is { ConfigMapRef: not null } package && !DnsSubdomain.IsMatch(package.ConfigMapRef))
        {
            problems.Add(new(InvalidSpec, "spec.package.configMapRef must be a valid ConfigMap name."));
        }

        var notes = spec.Notes;
        if (notes is { Enabled: true } && string.IsNullOrWhiteSpace(notes.CollectorUrl) != string.IsNullOrWhiteSpace(notes.CollectorTokenSecretRef))
        {
            problems.Add(new(InvalidSpec, "spec.notes.collectorUrl and spec.notes.collectorTokenSecretRef must be set together."));
        }

        var lifetime = spec.Lifetime;
        var permanent = lifetime?.Permanent == true;
        var hasUntil = !string.IsNullOrWhiteSpace(lifetime?.Until);
        if (permanent == hasUntil)
        {
            problems.Add(new(InvalidSpec, "spec.lifetime must set exactly one of permanent: true or until."));
        }
        else if (hasUntil && Rfc3339.Parse(lifetime!.Until) is null)
        {
            problems.Add(new(InvalidSpec, "spec.lifetime.until is not an RFC 3339 timestamp."));
        }

        return problems;
    }
}
