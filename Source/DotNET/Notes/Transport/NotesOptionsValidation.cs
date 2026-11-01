// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes;

/// <summary>
/// Checks the options that depend on the transport.
/// </summary>
public static class NotesOptionsValidation
{
    /// <summary>
    /// Describes what is wrong with the options of the HTTP transport.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <returns>The problems, none when the options are valid or the transport is not HTTP.</returns>
    public static IEnumerable<string> HttpProblems(NotesOptions options)
    {
        if (!options.Enabled || options.Transport != NotesTransport.Http)
        {
            yield break;
        }

        if (!Uri.TryCreate(options.CollectorUrl, UriKind.Absolute, out var url) || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
        {
            yield return "Notes' HTTP transport needs the collector's absolute http(s) URL: set NotesOptions.CollectorUrl.";
        }

        if (string.IsNullOrWhiteSpace(options.CollectorToken) && string.IsNullOrWhiteSpace(options.CollectorTokenFile))
        {
            yield return "Notes' HTTP transport needs the secret the collector knows the publication by: set NotesOptions.CollectorToken or NotesOptions.CollectorTokenFile.";
        }
        else if (!string.IsNullOrWhiteSpace(options.CollectorTokenFile) && !File.Exists(options.CollectorTokenFile))
        {
            yield return "Notes' HTTP transport reads the collector secret from NotesOptions.CollectorTokenFile, but there is no such file.";
        }

        if (options.RelayInterval <= TimeSpan.Zero)
        {
            yield return "Notes' relay polls at an interval: set NotesOptions.RelayInterval to more than zero.";
        }

        if (options.RelayBatchSize < 1)
        {
            yield return "Notes' relay sends at least one note per request: set NotesOptions.RelayBatchSize to 1 or more.";
        }
    }
}
