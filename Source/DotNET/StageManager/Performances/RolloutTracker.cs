// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Decides which release of a published application is deployed and records how each rollout ended. A release that is not ready in time is rolled back to the previous one, and is not tried again until the spec asks for something else.
/// </summary>
public static class RolloutTracker
{
    /// <summary>
    /// The most entries the version history keeps.
    /// </summary>
    public const int MaxVersions = 20;

    /// <summary>
    /// Chooses the release to deploy: the one the spec asks for, unless it already failed and a previous release can be kept running.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="target">The release the spec asks for.</param>
    /// <returns>The release to deploy.</returns>
    public static PublishedRelease ToDeploy(PerformanceStatus status, PublishedRelease target)
    {
        var failed = LastIndexOf(status.Versions, record => record.Outcome == RolloutOutcome.Failed && target.Is(record));
        if (failed < 0 || Stable(status) is not { } stable)
        {
            return target;
        }

        var rolledOutLater = status.Versions.Skip(failed + 1).Any(record => record.Outcome == RolloutOutcome.RolledOut && target.Is(record));
        return rolledOutLater ? target : new(stable.Version, stable.Image, stable.PackageConfigMap);
    }

    /// <summary>
    /// Records what a reconciliation saw of the deployed release.
    /// </summary>
    /// <param name="status">The status to update.</param>
    /// <param name="deployed">The release that was deployed.</param>
    /// <param name="target">The release the spec asks for.</param>
    /// <param name="ready">Whether the deployed release is ready.</param>
    /// <param name="now">The current time.</param>
    /// <param name="timeout">How long a rollout may take.</param>
    public static void Observe(PerformanceStatus status, PublishedRelease deployed, PublishedRelease target, bool ready, DateTimeOffset now, TimeSpan timeout)
    {
        var stable = Stable(status);
        if (deployed != target || IsStable(target, stable))
        {
            status.Rollout = null;
            status.RunningVersion = deployed.Version;
            return;
        }

        if (ready)
        {
            Add(status, Record(target, RolloutOutcome.RolledOut, now, string.Empty));
            status.Rollout = null;
            status.RunningVersion = target.Version;
            return;
        }

        Track(status, stable, new Observation(target, now, timeout));
    }

    static void Track(PerformanceStatus status, RolloutRecord? stable, Observation observation)
    {
        var target = observation.Target;
        if (status.Rollout is null || Describe(status.Rollout) != target)
        {
            status.Rollout = new() { Version = target.Version, Image = target.Image, PackageConfigMap = target.PackageConfigMap, StartedAt = observation.Now };
        }

        status.RunningVersion = stable?.Version ?? string.Empty;
        if (!HasTimedOut(status, observation))
        {
            return;
        }

        Add(status, Record(target, RolloutOutcome.Failed, observation.Now, $"The version was not ready within {(int)observation.Timeout.TotalSeconds} seconds."));
        if (stable is not null)
        {
            Add(status, Record(new(stable.Version, stable.Image, stable.PackageConfigMap), RolloutOutcome.RolledBack, observation.Now, $"Restored after version {target.Version} failed."));
            status.Rollout = null;
            status.RunningVersion = stable.Version;
        }
    }

    static bool IsStable(PublishedRelease target, RolloutRecord? stable) =>
        stable is not null && target.Is(stable);

    static bool HasTimedOut(PerformanceStatus status, Observation observation) =>
        observation.Now - status.Rollout!.StartedAt >= observation.Timeout && !AlreadyFailed(status, observation.Target);

    static bool AlreadyFailed(PerformanceStatus status, PublishedRelease target) =>
        status.Versions.LastOrDefault() is { } last && last.Outcome == RolloutOutcome.Failed && target.Is(last);

    static RolloutRecord? Stable(PerformanceStatus status) =>
        status.Versions.LastOrDefault(record => record.Outcome != RolloutOutcome.Failed);

    static PublishedRelease Describe(RolloutProgress progress) => new(progress.Version, progress.Image, progress.PackageConfigMap);

    static RolloutRecord Record(PublishedRelease release, RolloutOutcome outcome, DateTimeOffset now, string reason) => new()
    {
        Version = release.Version,
        Image = release.Image,
        PackageConfigMap = release.PackageConfigMap,
        Outcome = outcome,
        RolledOutAt = now,
        Reason = reason
    };

    static void Add(PerformanceStatus status, RolloutRecord record)
    {
        var versions = status.Versions.ToList();
        versions.Add(record);
        status.Versions = [.. versions.Skip(Math.Max(0, versions.Count - MaxVersions))];
    }

    static int LastIndexOf(IList<RolloutRecord> records, Func<RolloutRecord, bool> predicate)
    {
        for (var index = records.Count - 1; index >= 0; index--)
        {
            if (predicate(records[index]))
            {
                return index;
            }
        }

        return -1;
    }

    readonly record struct Observation(PublishedRelease Target, DateTimeOffset Now, TimeSpan Timeout);
}
