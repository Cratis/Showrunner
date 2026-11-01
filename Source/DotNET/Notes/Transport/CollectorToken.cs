// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport;

/// <summary>
/// Reads the bearer secret the collector knows the publication by.
/// </summary>
/// <remarks>
/// The file wins over the value in the options, and is read each time the secret is used so a rotated secret takes
/// effect without a restart. The secret is only ever handed to the HTTP request: nothing here, or in the relay, formats it
/// into a message.
/// </remarks>
/// <param name="options">Notes' options.</param>
public class CollectorToken(IOptions<NotesOptions> options)
{
    /// <summary>
    /// Reads the secret.
    /// </summary>
    /// <param name="cancellationToken">Stops waiting when the host shuts down.</param>
    /// <returns>The secret, trimmed.</returns>
    /// <exception cref="InvalidOperationException">There is no secret.</exception>
    public async Task<string> Read(CancellationToken cancellationToken = default)
    {
        var value = options.Value;
        var token = string.IsNullOrWhiteSpace(value.CollectorTokenFile)
            ? value.CollectorToken
            : await File.ReadAllTextAsync(value.CollectorTokenFile, cancellationToken);
        token = token.Trim();
        return token.Length == 0 ? throw new InvalidOperationException("The collector secret is empty.") : token;
    }
}
