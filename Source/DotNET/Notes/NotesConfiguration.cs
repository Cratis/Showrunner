// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Configuration;

namespace Cratis.Showrunner.Notes;

/// <summary>
/// Reads <see cref="NotesOptions"/> from configuration.
/// </summary>
/// <remarks>
/// The configuration binder cannot build concept types, so a plain <c>Bind</c> leaves the application name, the
/// publication, the version, the collecting stores and the collector tenant unset. This reads those from their text.
/// </remarks>
public static class NotesConfiguration
{
    /// <summary>
    /// Binds the options from a configuration section, such as <c>Notes</c>.
    /// </summary>
    /// <param name="options">The <see cref="NotesOptions"/> to bind.</param>
    /// <param name="section">The <see cref="IConfiguration"/> section holding the values.</param>
    /// <returns>The same <see cref="NotesOptions"/>, for chaining.</returns>
    public static NotesOptions BindFrom(this NotesOptions options, IConfiguration section)
    {
        section.Bind(options);
        if (Text(section, nameof(NotesOptions.ApplicationName)) is { } applicationName)
        {
            options.ApplicationName = new ApplicationName(applicationName);
        }

        if (Text(section, nameof(NotesOptions.PublicationId)) is { } publicationId)
        {
            options.PublicationId = new PublicationId(publicationId);
        }

        if (Text(section, nameof(NotesOptions.ApplicationVersion)) is { } applicationVersion)
        {
            options.ApplicationVersion = new ApplicationVersion(applicationVersion);
        }

        if (Text(section, nameof(NotesOptions.CollectorTenantId)) is { } collectorTenantId)
        {
            options.CollectorTenantId = new TenantId(collectorTenantId);
        }

        var stores = section.GetSection(nameof(NotesOptions.CollectingStores)).GetChildren()
            .Select(store => store.Value)
            .Where(store => !string.IsNullOrWhiteSpace(store))
            .Select(store => new EventStoreName(store!))
            .ToArray();
        if (stores.Length > 0)
        {
            options.CollectingStores = stores;
        }

        return options;
    }

    static string? Text(IConfiguration section, string key) =>
        section[key] is { Length: > 0 } value && !string.IsNullOrWhiteSpace(value) ? value : null;
}
