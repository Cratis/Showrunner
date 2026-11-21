// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operators;

/// <summary>
/// Renders the values of the operator chart from the inputs of a <see cref="StageManagerOperator"/>. Pure, so a spec can run the
/// result through <c>helm template</c> of the chart in this repository and catch a key that drifted from its <c>values.yaml</c>.
/// </summary>
public static class StageManagerOperatorValues
{
    /// <summary>
    /// Renders the values.
    /// </summary>
    /// <param name="args">The inputs.</param>
    /// <returns>The values, as nested dictionaries, lists and scalars.</returns>
    public static IDictionary<string, object> Render(StageManagerOperatorArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        var values = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["image"] = Image(args.OperatorImageRepository, args.Version),
            ["understudy"] = new Dictionary<string, object>(StringComparer.Ordinal) { ["image"] = Image(args.UnderstudyImageRepository, args.Version) }
        };
        if (args.ManagedNamespaces.Count > 0)
        {
            values["managedNamespaces"] = args.ManagedNamespaces.Cast<object>().ToList();
        }

        return Merge(values, args.Values);
    }

    /// <summary>
    /// Gets the operator image with its tag.
    /// </summary>
    /// <param name="args">The inputs.</param>
    /// <returns>The image reference.</returns>
    public static string OperatorImage(StageManagerOperatorArgs args) => $"{args.OperatorImageRepository ?? StageManagerOperator.DefaultOperatorImage}:{args.Version}";

    /// <summary>
    /// Gets the Understudy job image with its tag.
    /// </summary>
    /// <param name="args">The inputs.</param>
    /// <returns>The image reference.</returns>
    public static string UnderstudyImage(StageManagerOperatorArgs args) => $"{args.UnderstudyImageRepository ?? StageManagerOperator.DefaultUnderstudyImage}:{args.Version}";

    /// <summary>
    /// Gets the watchdog image with the tag of the version.
    /// </summary>
    /// <param name="args">The inputs.</param>
    /// <returns>The image reference.</returns>
    public static string WatchdogImage(StageManagerOperatorArgs args) => $"{args.WatchdogImageRepository ?? StageManagerOperator.DefaultWatchdogImage}:{args.Version}";

    /// <summary>
    /// Merges overrides over values: maps are merged key by key, everything else is replaced.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <param name="overrides">The overrides.</param>
    /// <returns>The merged values.</returns>
    internal static Dictionary<string, object> Merge(IDictionary<string, object> values, IDictionary<string, object> overrides)
    {
        var merged = new Dictionary<string, object>(values, StringComparer.Ordinal);
        foreach (var (key, value) in overrides)
        {
            merged[key] = value is IDictionary<string, object> map && merged.TryGetValue(key, out var existing) && existing is IDictionary<string, object> current
                ? Merge(current, map)
                : value;
        }

        return merged;
    }

    static Dictionary<string, object> Image(string? repository, string tag)
    {
        var image = new Dictionary<string, object>(StringComparer.Ordinal) { ["tag"] = tag };
        if (!string.IsNullOrEmpty(repository))
        {
            image["repository"] = repository;
        }

        return image;
    }
}
