// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.given;

/// <summary>
/// Releases and version histories the rollout specifications run on.
/// </summary>
public static class Releases
{
    public static readonly DateTimeOffset Start = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);
    public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    public static PublishedRelease V140 => new("1.4.0", "cratis/stage:5.3.0", "crm-package-1-4-0");

    public static PublishedRelease V150 => new("1.5.0", "cratis/stage:5.3.0", "crm-package-1-5-0");

    public static RolloutRecord Record(PublishedRelease release, RolloutOutcome outcome) => new()
    {
        Version = release.Version,
        Image = release.Image,
        PackageConfigMap = release.PackageConfigMap,
        Outcome = outcome,
        RolledOutAt = Start
    };

    public static PerformanceStatus RunningV140() => new()
    {
        RunningVersion = "1.4.0",
        Versions = [Record(V140, RolloutOutcome.RolledOut)]
    };

    public static PerformanceStatus V150FailedAndV140Restored()
    {
        var status = RunningV140();
        status.Versions = [.. status.Versions, Record(V150, RolloutOutcome.Failed), Record(V140, RolloutOutcome.RolledBack)];
        return status;
    }
}
#endif
