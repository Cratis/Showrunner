// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Protects the form post against cross-site forgery. The form carries a token only this process can make, for the person
/// the page was shown to and the time it was shown, so a post needs a page this process rendered for that person recently.
/// </summary>
/// <remarks>
/// The token needs no cookie. A cookie set on the page is not reliably sent back with the post: browsers withhold strict
/// same-site cookies after the sign-in redirect that leads to the page, and the post was then refused for a reason the
/// person could not see. Another site cannot read the page, so it cannot get a token.
/// </remarks>
/// <param name="clock">The <see cref="TimeProvider"/> that dates tokens.</param>
public class DirectForgery(TimeProvider clock)
{
    /// <summary>
    /// How long a form can be posted after the page was shown.
    /// </summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);

    static readonly TimeSpan _skew = TimeSpan.FromMinutes(1);

    readonly byte[] _key = RandomNumberGenerator.GetBytes(32);

    /// <summary>
    /// Makes the token a form carries.
    /// </summary>
    /// <param name="person">The subject of the person.</param>
    /// <returns>The token.</returns>
    public string TokenFor(string person) => TokenFor(clock.GetUtcNow().ToUnixTimeSeconds(), person);

    /// <summary>
    /// Checks the token a form carries.
    /// </summary>
    /// <param name="token">The token in the form, or empty.</param>
    /// <param name="person">The subject of the person.</param>
    /// <returns><see langword="true"/> when this process made the form for the person within <see cref="Lifetime"/>.</returns>
    public bool IsValid(string token, string person)
    {
        var separator = token.IndexOf('.', StringComparison.Ordinal);
        if (separator <= 0 ||
            !long.TryParse(token.AsSpan(0, separator), NumberStyles.None, CultureInfo.InvariantCulture, out var issued))
        {
            return false;
        }

        var age = clock.GetUtcNow() - DateTimeOffset.FromUnixTimeSeconds(Math.Min(issued, DateTimeOffset.MaxValue.ToUnixTimeSeconds()));
        return age >= -_skew &&
            age <= Lifetime &&
            CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(TokenFor(issued, person)), Encoding.UTF8.GetBytes(token));
    }

    string TokenFor(long issued, string person)
    {
        var stamp = issued.ToString(CultureInfo.InvariantCulture);
        return $"{stamp}.{Convert.ToHexString(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes($"{stamp}\n{person}")))}";
    }
}
