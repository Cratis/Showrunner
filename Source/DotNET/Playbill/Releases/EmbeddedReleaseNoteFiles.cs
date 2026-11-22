// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Showrunner.Playbill.Releases;

/// <summary>
/// Represents an implementation of <see cref="IReleaseNoteFiles"/> that reads the files embedded in an application's
/// assemblies at build time.
/// </summary>
/// <remarks>
/// The <c>Cratis.Showrunner.Playbill</c> package embeds the project's <c>ReleaseNotes</c> folder as resources named
/// <c>Playbill/&lt;path&gt;</c>. When the same path is embedded by more than one assembly, the first assembly wins.
/// </remarks>
public class EmbeddedReleaseNoteFiles : IReleaseNoteFiles
{
    /// <summary>
    /// The prefix of the name of every embedded release-note resource.
    /// </summary>
    public const string ResourcePrefix = "Playbill/";

    readonly Dictionary<string, (Assembly Assembly, string Resource)> _resources = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedReleaseNoteFiles"/> class.
    /// </summary>
    /// <param name="assemblies">The assemblies that hold the embedded files.</param>
    public EmbeddedReleaseNoteFiles(IEnumerable<Assembly> assemblies)
    {
        var fingerprint = new List<string>();
        foreach (var assembly in assemblies)
        {
            var found = false;
            foreach (var resource in assembly.GetManifestResourceNames().Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal)))
            {
                // Resource names carry the path of the platform the build ran on.
                var path = resource[ResourcePrefix.Length..].Replace('\\', '/');
                if (_resources.TryAdd(path, (assembly, resource)))
                {
                    found = true;
                }
            }

            if (found)
            {
                fingerprint.Add(assembly.ManifestModule.ModuleVersionId.ToString("N"));
            }
        }

        Paths = [.. _resources.Keys.Order(StringComparer.Ordinal)];
        Fingerprint = string.Join('-', fingerprint);
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> Paths { get; }

    /// <inheritdoc/>
    public string Fingerprint { get; }

    /// <inheritdoc/>
    public Stream Open(string path)
    {
        if (!_resources.TryGetValue(path, out var entry))
        {
            throw new FileNotFoundException($"The release-note file '{path}' is not embedded.", path);
        }

        return entry.Assembly.GetManifestResourceStream(entry.Resource)
            ?? throw new FileNotFoundException($"The release-note file '{path}' could not be read from {entry.Assembly.GetName().Name}.", path);
    }
}
