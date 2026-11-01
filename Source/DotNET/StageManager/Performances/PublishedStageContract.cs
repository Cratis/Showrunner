// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Produces the configuration of the Stage runtime in published mode for a Performance. Structure becomes environment variables; the storage connection string and the Notes token become files in mounted Secrets, and the package is a mounted ConfigMap.
/// </summary>
public static class PublishedStageContract
{
    /// <summary>
    /// The default key of the package archive in its ConfigMap.
    /// </summary>
    public const string DefaultPackageKey = "package.tar.gz";

    /// <summary>
    /// The port the Stage runtime listens on. The image exposes no other.
    /// </summary>
    public const int Port = 9090;

    /// <summary>
    /// Where the package ConfigMap is mounted.
    /// </summary>
    public const string PackageDirectory = "/stage/package-archive";

    /// <summary>
    /// Where the Notes token Secret is mounted.
    /// </summary>
    public const string NotesDirectory = "/run/secrets/notes";

    /// <summary>
    /// The file name of the Notes token inside <see cref="NotesDirectory"/>.
    /// </summary>
    public const string NotesTokenFile = "token";

    /// <summary>
    /// The file name of the storage connection string inside the application's secrets folder.
    /// </summary>
    public const string StorageFile = "Stage__Published__Storage__ConnectionString";

    /// <summary>
    /// The path Stage reads to check that it is ready to serve.
    /// </summary>
    public const string ReadinessPath = "/stage/status";

    /// <summary>
    /// Determines whether the Performance runs a published Stage application.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>True when it does.</returns>
    public static bool IsPublished(PerformanceSpec spec) => spec.Application.Published is not null;

    /// <summary>
    /// Determines whether the published application hosts Notes.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>True when Notes is enabled.</returns>
    public static bool NotesEnabled(PerformanceSpec spec) => spec.Application.Published?.Notes is { Enabled: true };

    /// <summary>
    /// Gets the key of the package archive in its ConfigMap.
    /// </summary>
    /// <param name="published">The published settings.</param>
    /// <returns>The key.</returns>
    public static string PackageKey(PublishedSpec published) =>
        string.IsNullOrEmpty(published.PackageConfigMapRef.Key) ? DefaultPackageKey : published.PackageConfigMapRef.Key;

    /// <summary>
    /// Gets the environment variables of the Stage container.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <param name="secretsDirectory">Where the application's secrets folder is mounted.</param>
    /// <returns>The variables by name.</returns>
    public static IReadOnlyDictionary<string, string> EnvironmentVariables(Performance performance, string secretsDirectory)
    {
        var spec = performance.Spec;
        var published = spec.Application.Published!;
        var variables = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Stage__Mode"] = "Published",
            ["Stage__Published__PackagePath"] = $"{PackageDirectory}/{PackageKey(published)}",
            ["Stage__Published__Storage__ConnectionStringFile"] = $"{secretsDirectory}/{StorageFile}",
            ["Stage__Published__Authentication__Mode"] = "TrustedProxy",
            ["Stage__Published__Notes__Enabled"] = NotesEnabled(spec) ? "true" : "false",
            ["AllowedHosts"] = string.Join(';', spec.Ingress.Hosts)
        };

        for (var index = 0; index < published.TrustedProxies.Count; index++)
        {
            variables[$"Stage__Published__TrustedProxies__{index}"] = published.TrustedProxies[index];
        }

        if (published.Notes is { Enabled: true } notes)
        {
            variables["Notes__Transport"] = "Http";
            variables["Notes__CollectorUrl"] = notes.CollectorUrl;
            variables["Notes__CollectorTokenFile"] = $"{NotesDirectory}/{NotesTokenFile}";
            variables["Notes__PublicationId"] = notes.PublicationId;
            variables["Notes__ApplicationName"] = notes.ApplicationName;
            if (!string.IsNullOrEmpty(notes.CollectorTenantId))
            {
                variables["Notes__CollectorTenantId"] = notes.CollectorTenantId;
            }
        }

        return variables;
    }

    /// <summary>
    /// Gets the resources of the Stage container, with the defaults filled in.
    /// </summary>
    /// <param name="published">The published settings.</param>
    /// <returns>The resource requirements.</returns>
    public static V1ResourceRequirements Resources(PublishedSpec published) => new()
    {
        Requests = Quantities(published.Resources.Requests, new() { ["cpu"] = "250m", ["memory"] = "512Mi" }),
        Limits = Quantities(published.Resources.Limits, new() { ["cpu"] = "1", ["memory"] = "1Gi" })
    };

    static Dictionary<string, ResourceQuantity> Quantities(IDictionary<string, string> given, Dictionary<string, string> defaults)
    {
        var merged = new Dictionary<string, string>(defaults, StringComparer.Ordinal);
        foreach (var (name, quantity) in given)
        {
            merged[name] = quantity;
        }

        return merged.ToDictionary(pair => pair.Key, pair => new ResourceQuantity(pair.Value), StringComparer.Ordinal);
    }
}
