// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Showrunner.Playbill;

/// <summary>
/// The options of Playbill.
/// </summary>
public class PlaybillOptions
{
    /// <summary>
    /// Gets the assemblies that hold the embedded release notes. When none is named, the application's entry assembly is
    /// read: the project that was built with a <c>ReleaseNotes</c> folder.
    /// </summary>
    public IList<Assembly> Assemblies { get; } = [];

    /// <summary>
    /// Gets or sets the version the application is running. When it is not set, the informational version of the first
    /// assembly is used, without its build metadata.
    /// </summary>
    public string? CurrentVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the endpoints under <c>/.cratis/playbill</c> answer callers who are not
    /// signed in. They require a signed-in user unless this is set, because release notes can describe behavior that is
    /// not for everyone.
    /// </summary>
    public bool AllowAnonymous { get; set; }

    /// <summary>
    /// Gets the assemblies the release notes are read from.
    /// </summary>
    /// <returns>The assemblies that were named, or the entry assembly when none was.</returns>
    public IReadOnlyList<Assembly> ResolveAssemblies()
    {
        if (Assemblies.Count > 0)
        {
            return [.. Assemblies];
        }

        return Assembly.GetEntryAssembly() is { } entry ? [entry] : [];
    }

    /// <summary>
    /// Gets the version the application is running.
    /// </summary>
    /// <returns>The configured version, or the version the assembly was built with, or <see cref="ReleaseVersion.None"/>.</returns>
    public ReleaseVersion ResolveCurrentVersion()
    {
        if (!string.IsNullOrWhiteSpace(CurrentVersion))
        {
            return CurrentVersion.Trim();
        }

        var assemblies = ResolveAssemblies();
        var informational = (assemblies.Count > 0 ? assemblies[0] : null)?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrWhiteSpace(informational))
        {
            return ReleaseVersion.None;
        }

        var plus = informational.IndexOf('+');
        return plus >= 0 ? informational[..plus] : informational;
    }
}
