// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Protects the form post against cross-site forgery. A nonce goes to the browser in a cookie, and the form carries a
/// token only this process can make from that nonce and the person, so a post needs both.
/// </summary>
public class DirectForgery
{
    /// <summary>
    /// The name of the cookie that carries the nonce.
    /// </summary>
    public const string CookieName = "__Host-showrunner-direct";

    readonly byte[] _key = RandomNumberGenerator.GetBytes(32);

    /// <summary>
    /// Makes a nonce for a browser.
    /// </summary>
    /// <returns>The nonce.</returns>
    public static string NewNonce() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    /// <summary>
    /// Makes the token a form carries.
    /// </summary>
    /// <param name="nonce">The nonce in the cookie.</param>
    /// <param name="person">The subject of the person.</param>
    /// <returns>The token.</returns>
    public string TokenFor(string nonce, string person) =>
        Convert.ToHexString(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes($"{nonce}\n{person}")));

    /// <summary>
    /// Checks the token a form carries.
    /// </summary>
    /// <param name="nonce">The nonce in the cookie, or empty.</param>
    /// <param name="token">The token in the form, or empty.</param>
    /// <param name="person">The subject of the person.</param>
    /// <returns><see langword="true"/> when the post is the one this process made the form for.</returns>
    public bool IsValid(string nonce, string token, string person) =>
        nonce.Length > 0 &&
        token.Length > 0 &&
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(TokenFor(nonce, person)), Encoding.UTF8.GetBytes(token));
}
