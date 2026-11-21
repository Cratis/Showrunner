// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// The custom pages AuthProxy shows instead of its built-in ones: 403.html, select-tenant.html, styles.css and so on.
/// </summary>
/// <remarks>
/// Give exactly one of <see cref="ConfigMapName"/>, <see cref="Files"/> and <see cref="Directory"/>. Files are text: a ConfigMap
/// holds no binary content, so a logo is an SVG or lives on another host.
/// </remarks>
public sealed class AuthProxyPagesArgs
{
    /// <summary>
    /// Gets the name of an existing ConfigMap that holds the pages.
    /// </summary>
    public string? ConfigMapName { get; init; }

    /// <summary>
    /// Gets the pages as text by file name. The component writes them to a ConfigMap.
    /// </summary>
    public IDictionary<string, Input<string>>? Files { get; init; }

    /// <summary>
    /// Gets a directory of pages. The component reads the files in it when the program runs and writes them to a ConfigMap.
    /// </summary>
    public string? Directory { get; init; }

    /// <summary>
    /// Builds the spec of the pages, writing the ConfigMap when the pages are given as values.
    /// </summary>
    /// <param name="context">The context of the Performance.</param>
    /// <returns>The spec.</returns>
    /// <exception cref="ArgumentException">Thrown when the pages are not given as exactly one of the three ways.</exception>
    internal Dictionary<string, object> ToSpec(PerformanceContext context)
    {
        if (new object?[] { ConfigMapName, Files, Directory }.Count(source => source is not null) != 1)
        {
            throw new ArgumentException("Give the pages of AuthProxy as exactly one of ConfigMapName, Files and Directory.");
        }

        var name = ConfigMapName ?? PerformanceFiles.Declare(context, "authproxy-pages", Files is not null ? new Dictionary<string, Input<string>>(Files, StringComparer.Ordinal) : PerformanceFiles.FromDirectory(Directory!));
        return SpecExtensions.NewSpec().Put("configMapRef", SpecExtensions.Reference(name, null));
    }
}
