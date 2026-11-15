// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// The verdict on the rules of a bucket that holds key backups.
/// </summary>
/// <param name="Holds">Whether every rule holds.</param>
/// <param name="Reason">A short reason in CamelCase, usable as a condition reason.</param>
/// <param name="Message">What is wrong, or that nothing is.</param>
public record BucketRulesCheck(bool Holds, string Reason, string Message)
{
    /// <summary>
    /// The reason when every rule holds.
    /// </summary>
    public const string RulesHold = "BucketRulesHold";

    /// <summary>
    /// The reason when versioning is on, which would keep deleted keys.
    /// </summary>
    public const string Versioned = "BucketVersioned";

    /// <summary>
    /// The reason when object lock is on, which would stop keys from being deleted.
    /// </summary>
    public const string ObjectLocked = "BucketObjectLocked";

    /// <summary>
    /// The reason when the provider cannot prove the rules.
    /// </summary>
    public const string NotProvable = "BucketRulesNotProvable";

    /// <summary>
    /// The reason when the expiry rule is not in place after it was applied.
    /// </summary>
    public const string ExpiryMissing = "BucketExpiryMissing";
}
