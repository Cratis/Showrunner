// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Reporting.Capture;

/// <summary>
/// Resolves the version of the application errors are captured in.
/// </summary>
public static class ApplicationVersions
{
    static readonly Lazy<ApplicationVersion> _entryAssemblyVersion = new(() => FromAssembly(Assembly.GetEntryAssembly()));

    /// <summary>
    /// Resolves the application version: <see cref="UsherOptions.ApplicationVersion"/> when it is set, otherwise the
    /// version of the entry assembly.
    /// </summary>
    /// <param name="options">The <see cref="UsherOptions"/>.</param>
    /// <returns>The <see cref="ApplicationVersion"/>; <see cref="ApplicationVersion.NotSet"/> when none is known.</returns>
    public static ApplicationVersion Resolve(UsherOptions options) =>
        options.ApplicationVersion != ApplicationVersion.NotSet ? options.ApplicationVersion : _entryAssemblyVersion.Value;

    /// <summary>
    /// Gets the version of an assembly: its informational version without build metadata (the part after <c>+</c>),
    /// otherwise its assembly version.
    /// </summary>
    /// <param name="assembly">The <see cref="Assembly"/>, if any.</param>
    /// <returns>The <see cref="ApplicationVersion"/>; <see cref="ApplicationVersion.NotSet"/> when none is known.</returns>
    public static ApplicationVersion FromAssembly(Assembly? assembly)
    {
        var informational = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            var metadata = informational.IndexOf('+');
            return (metadata >= 0 ? informational[..metadata] : informational).Trim();
        }

        return assembly?.GetName().Version?.ToString() ?? ApplicationVersion.NotSet;
    }
}
