// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// Static files Ante serves itself under <c>/brand</c>. Give exactly one of <see cref="ConfigMapName"/> with <see cref="FileNames"/>,
/// <see cref="Files"/> and <see cref="Directory"/>. Files are text, so a logo is an SVG or lives on another host.
/// </summary>
public sealed class AnteBrandingAssetsArgs
{
    /// <summary>
    /// Gets the name of an existing ConfigMap that holds the files. Name its keys in <see cref="FileNames"/>.
    /// </summary>
    public string? ConfigMapName { get; init; }

    /// <summary>
    /// Gets the keys of the existing ConfigMap to serve.
    /// </summary>
    public IList<string> FileNames { get; init; } = [];

    /// <summary>
    /// Gets the files as text by file name. The component writes them to a ConfigMap.
    /// </summary>
    public IDictionary<string, Input<string>>? Files { get; init; }

    /// <summary>
    /// Gets a directory of files. The component reads the files in it when the program runs and writes them to a ConfigMap.
    /// </summary>
    public string? Directory { get; init; }

    /// <summary>
    /// Builds the spec of the assets, writing the ConfigMap when the files are given as values.
    /// </summary>
    /// <param name="context">The context of the Performance.</param>
    /// <returns>The spec.</returns>
    /// <exception cref="ArgumentException">Thrown when the files are not given as exactly one of the three ways.</exception>
    internal Dictionary<string, object> ToSpec(PerformanceContext context)
    {
        if (new object?[] { ConfigMapName, Files, Directory }.Count(source => source is not null) != 1)
        {
            throw new ArgumentException("Give the branding assets of Ante as exactly one of ConfigMapName, Files and Directory.");
        }

        if (ConfigMapName is not null)
        {
            return Spec(ConfigMapName, FileNames);
        }

        var files = Files is not null ? new Dictionary<string, Input<string>>(Files, StringComparer.Ordinal) : PerformanceFiles.FromDirectory(Directory!);
        return Spec(PerformanceFiles.Declare(context, "brand", files), [.. files.Keys.Order(StringComparer.Ordinal)]);
    }

    static Dictionary<string, object> Spec(string configMapName, IEnumerable<string> files) =>
        SpecExtensions.NewSpec()
            .Put("configMapRef", SpecExtensions.Reference(configMapName, null))
            .PutList("files", files);
}
