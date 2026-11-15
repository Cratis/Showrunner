// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// The condition types of an <see cref="Understudy"/>.
/// </summary>
public static class UnderstudyConditions
{
    /// <summary>
    /// The spec is valid.
    /// </summary>
    public const string SpecValid = "SpecValid";

    /// <summary>
    /// The Production the Understudy names exists.
    /// </summary>
    public const string ProductionFound = "ProductionFound";

    /// <summary>
    /// The credentials the storage names are in hand and the bucket can be reached.
    /// </summary>
    public const string StorageReady = "StorageReady";

    /// <summary>
    /// The bucket expires every object after the maximum age, so retention holds even when the operator is down.
    /// </summary>
    public const string LifecycleEnforced = "LifecycleEnforced";

    /// <summary>
    /// The backups are scheduled.
    /// </summary>
    public const string Scheduled = "Scheduled";

    /// <summary>
    /// The latest backup run succeeded.
    /// </summary>
    public const string BackupsHealthy = "BackupsHealthy";

    /// <summary>
    /// The keys bucket holds the rules key backups need, and the inputs of the key backups are in hand.
    /// </summary>
    public const string KeyBackupsReady = "KeyBackupsReady";
}
