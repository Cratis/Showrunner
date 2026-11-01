// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Showrunner.Notes;

/// <summary>
/// Describes the application a note is given in, from the options the host configured Notes with.
/// </summary>
public static class PublishedApplications
{
    static readonly Lazy<ApplicationVersion> _entryAssemblyVersion = new(() => VersionOf(Assembly.GetEntryAssembly()));

    /// <summary>
    /// Describes the application.
    /// </summary>
    /// <param name="options">The <see cref="NotesOptions"/>.</param>
    /// <returns>The <see cref="PublishedApplication"/>, with <see cref="NotesOptions.ApplicationVersion"/> when it is set and otherwise the version of the entry assembly.</returns>
    public static PublishedApplication Of(NotesOptions options) => new(
        options.ApplicationName,
        options.ApplicationVersion != ApplicationVersion.NotSet ? options.ApplicationVersion : _entryAssemblyVersion.Value,
        options.PublicationId);

    /// <summary>
    /// Gets the version of an assembly: its informational version without build metadata (the part after <c>+</c>),
    /// otherwise its assembly version.
    /// </summary>
    /// <param name="assembly">The <see cref="Assembly"/>, if any.</param>
    /// <returns>The <see cref="ApplicationVersion"/>; <see cref="ApplicationVersion.NotSet"/> when none is known.</returns>
    public static ApplicationVersion VersionOf(Assembly? assembly)
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
