// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Storage;

/// <summary>
/// Checks the bucket that holds key backups before any key is touched: versioning off, no object lock, and an expiry rule that is applied and read back.
/// A key that was backed up and later erased must not survive in the bucket, so a bucket that cannot prove this refuses key backups.
/// </summary>
public static class KeyBucketGuard
{
    /// <summary>
    /// Checks the rules, applying the expiry rule and reading it back.
    /// </summary>
    /// <param name="storage">The storage of the keys bucket.</param>
    /// <param name="expiryDays">The days after which keys expire.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The verdict.</returns>
    public static async Task<BucketRulesCheck> EnsureAsync(IObjectStorage storage, int expiryDays, CancellationToken cancellationToken)
    {
        var before = await storage.InspectAsync(cancellationToken);
        if (before.VersioningEnabled == true)
        {
            return new(false, BucketRulesCheck.Versioned, "The bucket has versioning on, which would keep a key after it was deleted. Turn versioning off or use another bucket.");
        }

        if (before.ObjectLockEnabled == true)
        {
            return new(false, BucketRulesCheck.ObjectLocked, "The bucket has object lock on, which would stop keys from being deleted. Use a bucket without object lock.");
        }

        if (before.VersioningEnabled is null || before.ObjectLockEnabled is null)
        {
            return new(false, BucketRulesCheck.NotProvable, $"The {storage.Provider} provider cannot show that versioning and object lock are off, so it cannot hold key backups.");
        }

        if (!await storage.TryEnforceExpiryAsync(expiryDays, cancellationToken))
        {
            return new(false, BucketRulesCheck.NotProvable, $"The {storage.Provider} provider cannot apply an expiry rule, so it cannot hold key backups.");
        }

        var after = await storage.InspectAsync(cancellationToken);
        return after.ExpiryDays == expiryDays
            ? new(true, BucketRulesCheck.RulesHold, $"Versioning and object lock are off and keys expire after {expiryDays} days.")
            : new(false, BucketRulesCheck.ExpiryMissing, "The expiry rule was applied but is not what the bucket reports when it is read back.");
    }
}
