// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// Checks the spec of an <see cref="Understudy"/> before anything is scheduled. A problem blocks the Understudy; nothing is applied until the spec is valid.
/// </summary>
public static partial class UnderstudyValidator
{
    [GeneratedRegex(@"^(@(hourly|daily|weekly|monthly|yearly)|([0-9*/,\-?A-Za-z]+\s+){4}[0-9*/,\-?A-Za-z]+)$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 200)]
    private static partial Regex CronPattern { get; }

    /// <summary>
    /// Validates the spec.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>The problems, empty when the spec is valid.</returns>
    public static IReadOnlyList<UnderstudyProblem> Validate(UnderstudySpec spec)
    {
        var problems = new List<UnderstudyProblem>();
        if (string.IsNullOrWhiteSpace(spec.ProductionRef))
        {
            problems.Add(new("ProductionRefMissing", "productionRef names no Production."));
        }

        if (!IsCron(spec.Schedule))
        {
            problems.Add(new("InvalidSchedule", "schedule is not a cron expression of five fields, such as '0 * * * *'."));
        }

        if (spec.MaxAgeDays < 1)
        {
            problems.Add(new("InvalidMaxAge", "maxAgeDays must be at least 1."));
        }

        var retention = spec.Retention;
        if (retention.Hourly < 0 || retention.Daily < 0 || retention.Weekly < 0 || retention.Monthly < 0 || retention.Yearly < 0)
        {
            problems.Add(new("InvalidRetention", "The retention tiers cannot be negative."));
        }

        problems.AddRange(ValidateStorage("storage", spec.Storage));
        if (spec.Keys is { } keys)
        {
            problems.AddRange(ValidateKeys(spec, keys));
        }

        return problems;
    }

    /// <summary>
    /// Gets whether two storages are the same bucket, which the key backups must never share with the data backups.
    /// </summary>
    /// <param name="first">The first storage.</param>
    /// <param name="second">The second storage.</param>
    /// <returns>True when they name the same bucket.</returns>
    public static bool SameBucket(UnderstudyStorage first, UnderstudyStorage second) =>
        string.Equals(first.Provider, second.Provider, StringComparison.Ordinal) &&
        string.Equals(first.Endpoint, second.Endpoint, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(first.Bucket, second.Bucket, StringComparison.Ordinal) &&
        string.Equals(first.VolumeClaimName, second.VolumeClaimName, StringComparison.Ordinal);

    static bool IsCron(string schedule) => !string.IsNullOrWhiteSpace(schedule) && CronPattern.IsMatch(schedule.Trim());

    static IEnumerable<UnderstudyProblem> ValidateStorage(string path, UnderstudyStorage storage)
    {
        if (!ObjectStorageProviders.IsSupported(storage.Provider))
        {
            yield return new("UnsupportedProvider", $"{path}.provider '{storage.Provider}' is not supported. Use S3, AzureBlob or Volume.");
            yield break;
        }

        if (string.Equals(storage.Provider, ObjectStorageProviders.Volume, StringComparison.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(storage.VolumeClaimName))
            {
                yield return new("VolumeClaimMissing", $"{path}.volumeClaimName names no PersistentVolumeClaim.");
            }

            yield break;
        }

        if (string.IsNullOrWhiteSpace(storage.Bucket))
        {
            yield return new("BucketMissing", $"{path}.bucket names no bucket.");
        }

        if (string.IsNullOrWhiteSpace(storage.CredentialsSecretName))
        {
            yield return new("CredentialsMissing", $"{path}.credentialsSecretName names no Secret.");
        }
    }

    static IEnumerable<UnderstudyProblem> ValidateKeys(UnderstudySpec spec, UnderstudyKeys keys)
    {
        foreach (var problem in ValidateStorage("keys.storage", keys.Storage))
        {
            yield return problem;
        }

        if (SameBucket(spec.Storage, keys.Storage))
        {
            yield return new("KeysBucketShared", "keys.storage is the bucket of the data backups. Key backups need a bucket of their own, with their own credentials.");
        }

        if (!IsCron(keys.Schedule))
        {
            yield return new("InvalidKeysSchedule", "keys.schedule is not a cron expression of five fields.");
        }

        if (keys.MaxAgeDays < 1)
        {
            yield return new("InvalidKeysMaxAge", "keys.maxAgeDays must be at least 1.");
        }

        if (string.IsNullOrWhiteSpace(keys.CertificateSecret.Name))
        {
            yield return new("CertificateMissing", "keys.certificateSecret names no Secret. Keys are encrypted to the customer's public certificate.");
        }

        if (string.IsNullOrWhiteSpace(keys.VaultTokenSecret.Name))
        {
            yield return new("VaultTokenMissing", "keys.vaultTokenSecret names no Secret.");
        }
    }
}
