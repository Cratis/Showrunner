// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// The rules a bucket is under, as the provider reports them. Null means the provider cannot tell, which never counts as the rule holding.
/// </summary>
/// <param name="VersioningEnabled">Whether object versioning is on; a versioned bucket keeps deleted keys.</param>
/// <param name="ObjectLockEnabled">Whether object lock is on; locked objects cannot be deleted.</param>
/// <param name="ExpiryDays">The days after which every object expires, or null when no expiry rule is in place.</param>
public record BucketState(bool? VersioningEnabled, bool? ObjectLockEnabled, int? ExpiryDays)
{
    /// <summary>
    /// The state of a provider that can prove none of the rules.
    /// </summary>
    public static readonly BucketState Unknown = new(null, null, null);
}
