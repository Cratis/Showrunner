// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Computes a revision of mounted content, so a workload rolls when it changes. The revision is a keyed hash, so it reveals nothing about a value even when the value is easy to guess.
/// </summary>
public static class ConfigRevision
{
    /// <summary>
    /// Computes the revision of a set of files.
    /// </summary>
    /// <param name="key">The secret key to hash with; a generated private key works well.</param>
    /// <param name="files">The files by name.</param>
    /// <returns>A short hexadecimal revision.</returns>
    public static string Of(string key, IReadOnlyDictionary<string, string> files)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        foreach (var (name, content) in files.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            hmac.TransformBlock(Encoding.UTF8.GetBytes(name), 0, Encoding.UTF8.GetByteCount(name), null, 0);
            var bytes = Encoding.UTF8.GetBytes(content + "\n");
            hmac.TransformBlock(bytes, 0, bytes.Length, null, 0);
        }

        hmac.TransformFinalBlock([], 0, 0);
        return Convert.ToHexStringLower(hmac.Hash!)[..16];
    }
}
