// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Jobs;

/// <summary>
/// What a backup job can be asked to do. It is the first argument of the job image.
/// </summary>
public static class UnderstudyJobKinds
{
    /// <summary>
    /// The argument that starts the job instead of the operator.
    /// </summary>
    public const string Command = "understudy";

    /// <summary>
    /// Back up the databases and prune.
    /// </summary>
    public const string Backup = "backup";

    /// <summary>
    /// Back up the keys of every namespace, encrypted to the customer's certificate.
    /// </summary>
    public const string Keys = "keys";

    /// <summary>
    /// Take a snapshot of the whole Vault, encrypted to the customer's certificate.
    /// </summary>
    public const string VaultSnapshot = "vault-snapshot";
}
