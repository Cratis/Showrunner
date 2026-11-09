// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Generates the passwords and client secrets a Production keeps in its Secrets.
/// </summary>
public static class GeneratedSecret
{
    /// <summary>
    /// Generates a random value of 32 bytes, URL-safe so it can stand in a connection string unescaped.
    /// </summary>
    /// <returns>The value.</returns>
    public static string New() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
