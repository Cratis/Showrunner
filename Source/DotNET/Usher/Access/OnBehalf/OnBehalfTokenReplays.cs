// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Remembers the identifiers (<c>jti</c>) of the AuthProxy tokens this instance accepted, until the tokens expire, so
/// a token is accepted once.
/// </summary>
/// <remarks>
/// What it remembers is process-wide, however many instances of it a container creates, so a token is accepted once
/// whichever way the type was registered. It holds no tenant data: a token identifier is random and unique across
/// tenants. It is per process; a token replayed against another replica of the application within its lifetime is
/// still bound to its call, its purpose and its body.
/// </remarks>
/// <param name="time">The clock.</param>
public class OnBehalfTokenReplays(TimeProvider time)
{
    static readonly ConcurrentDictionary<string, DateTimeOffset> _seen = new(StringComparer.Ordinal);

    /// <summary>
    /// Uses a token identifier.
    /// </summary>
    /// <param name="tokenId">The token identifier.</param>
    /// <param name="until">When the token expires, after which it no longer needs remembering.</param>
    /// <returns>True when it was not used before; false when it is a replay.</returns>
    public bool TryUse(string tokenId, DateTimeOffset until)
    {
        var now = time.GetUtcNow();
        foreach (var expired in _seen.Where(_ => _.Value <= now))
        {
            _seen.TryRemove(expired);
        }

        return _seen.TryAdd(tokenId, until);
    }
}
