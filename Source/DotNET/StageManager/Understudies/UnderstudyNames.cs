// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// The names of everything an <see cref="Understudy"/> owns, all derived from the name of the Understudy.
/// </summary>
/// <param name="Understudy">The name of the Understudy.</param>
public readonly record struct UnderstudyNames(string Understudy)
{
    /// <summary>
    /// Gets the name of the CronJob that backs up the databases.
    /// </summary>
    public string BackupCronJob => $"{Understudy}-backup";

    /// <summary>
    /// Gets the name of the CronJob that backs up the keys.
    /// </summary>
    public string KeysCronJob => $"{Understudy}-keys";

    /// <summary>
    /// Gets the name of the job that takes a Vault snapshot for a request.
    /// </summary>
    /// <param name="request">The value of the request annotation, so a new value is a new job.</param>
    /// <returns>The name.</returns>
    public string VaultSnapshotJob(string request) =>
        $"{Understudy}-vault-{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(request)))[..10]}";

    /// <summary>
    /// Gets the names of an Understudy.
    /// </summary>
    /// <param name="understudy">The Understudy.</param>
    /// <returns>The names.</returns>
    public static UnderstudyNames For(Understudy understudy) => new(understudy.Metadata.Name);
}
