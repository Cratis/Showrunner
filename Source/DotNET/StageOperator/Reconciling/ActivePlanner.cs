// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>Plans a reconcile of an application that should be running: rollout, rollback and steady state.</summary>
public static class ActivePlanner
{
    static readonly TimeSpan _rolloutPoll = TimeSpan.FromSeconds(5);

    /// <summary>Plans the reconcile.</summary>
    /// <param name="context">The facts to decide from.</param>
    /// <returns>The plan.</returns>
    public static ReconcilePlan Plan(PlanningContext context)
    {
        var previous = context.Previous;
        var spec = context.Spec;
        var lastGood = previous.Versions.LastOrDefault(v => v.Outcome == StageRolloutOutcomes.RolledOut);
        var rollout = previous.Rollout;

        if (rollout is not null && RolloutStillWanted(previous, spec, rollout))
        {
            return ContinueRollout(context, rollout, lastGood);
        }

        if (IsHeldBack(previous, spec))
        {
            return HoldAtLastGood(context, lastGood);
        }

        if (previous.RunningVersion == spec.Version && previous.RunningImage == spec.Image && previous.RunningPackage == PackageOf(spec))
        {
            return Steady(context, spec.Image, spec.Version, PackageOf(spec));
        }

        var started = new StageApplicationRollout
        {
            TargetVersion = spec.Version,
            TargetImage = spec.Image,
            TargetPackage = PackageOf(spec),
            StartedAt = Rfc3339.Format(context.Now),
            IsRollback = false
        };

        return InProgress(context, started, previous.Versions);
    }

    static bool RolloutStillWanted(StageApplicationStatus previous, StageApplicationSpec spec, StageApplicationRollout rollout) =>
        rollout.IsRollback
            ? IsHeldBack(previous, spec)
            : rollout.TargetVersion == spec.Version && rollout.TargetImage == spec.Image && rollout.TargetPackage == PackageOf(spec);

    /// <summary>
    /// A version that failed and was rolled back (or failed with nothing to roll back to) stays held back until the
    /// spec asks for something else; otherwise the same failing image would be rolled out again forever.
    /// </summary>
    /// <param name="previous">The status.</param>
    /// <param name="spec">The spec.</param>
    /// <returns>True when the spec asks for the version that failed last.</returns>
    static bool IsHeldBack(StageApplicationStatus previous, StageApplicationSpec spec)
    {
        if (previous.Versions.Count == 0)
        {
            return false;
        }

        var last = previous.Versions[^1];
        var failed = last.Outcome == StageRolloutOutcomes.RolledBack || last.Outcome == StageRolloutOutcomes.Failed;
        return failed && last.Version == spec.Version && last.Image == spec.Image && last.Package == PackageOf(spec);
    }

    static string PhaseOf(PlanningContext context, StageApplicationRollout rollout)
    {
        if (rollout.IsRollback)
        {
            return StageApplicationPhases.RollingBack;
        }

        return context.Observed.Deployment is null ? StageApplicationPhases.Pending : StageApplicationPhases.Deploying;
    }

    static ReconcilePlan ContinueRollout(PlanningContext context, StageApplicationRollout rollout, StageApplicationVersion? lastGood)
    {
        var observed = context.Observed.Deployment;
        if (observed?.IsRolledOutAt(rollout.TargetImage, rollout.TargetPackage) == true)
        {
            return CompleteRollout(context, rollout);
        }

        var startedAt = Rfc3339.Parse(rollout.StartedAt) ?? context.Now;
        var timedOut = context.Now - startedAt > context.Options.RolloutTimeout
            || (observed?.ProgressDeadlineExceeded == true && observed.StageImage == rollout.TargetImage && observed.StagePackage == rollout.TargetPackage);

        if (!timedOut)
        {
            return InProgress(context, rollout, context.Previous.Versions);
        }

        return rollout.IsRollback ? FailRollback(context, rollout) : FailRollout(context, rollout, lastGood);
    }

    static ReconcilePlan CompleteRollout(PlanningContext context, StageApplicationRollout rollout)
    {
        var previous = context.Previous;
        var versions = previous.Versions;
        var conditions = context.Conditions();

        if (!rollout.IsRollback)
        {
            versions = Append(versions, new StageApplicationVersion
            {
                Version = rollout.TargetVersion,
                Image = rollout.TargetImage,
                Package = rollout.TargetPackage,
                RolledOutAt = Rfc3339.Format(context.Now),
                Outcome = StageRolloutOutcomes.RolledOut
            });
            conditions.Set(ConditionTypes.Ready, true, "RolledOut", $"Version {rollout.TargetVersion} is running.");
        }
        else
        {
            conditions
                .Set(ConditionTypes.Ready, true, "RolledBack", $"Version {rollout.TargetVersion} is running again.")
                .Set(ConditionTypes.Degraded, true, "RolledBack", $"Version {context.Spec.Version} did not become ready and was rolled back.");
        }

        var status = context.Carry() with
        {
            Phase = StageApplicationPhases.Running,
            RunningVersion = rollout.TargetVersion,
            RunningImage = rollout.TargetImage,
            RunningPackage = rollout.TargetPackage,
            Rollout = null,
            Versions = versions,
            Conditions = conditions.Build()
        };

        return new ReconcilePlan(status, Apply: Desired(context, rollout.TargetImage, rollout.TargetPackage));
    }

    static ReconcilePlan FailRollout(PlanningContext context, StageApplicationRollout rollout, StageApplicationVersion? lastGood)
    {
        var reason = $"Version {rollout.TargetVersion} did not become ready within {context.Options.RolloutTimeout}.";
        if (lastGood is null)
        {
            var failed = Append(context.Previous.Versions, new StageApplicationVersion
            {
                Version = rollout.TargetVersion,
                Image = rollout.TargetImage,
                Package = rollout.TargetPackage,
                RolledOutAt = Rfc3339.Format(context.Now),
                Outcome = StageRolloutOutcomes.Failed,
                Reason = reason
            });

            var failedConditions = context.Conditions()
                .Set(ConditionTypes.Degraded, true, "RolloutFailed", reason + " There was no earlier version to roll back to.")
                .Build();

            return new ReconcilePlan(
                context.Carry() with { Phase = StageApplicationPhases.Failed, Rollout = null, Versions = failed, Conditions = failedConditions },
                Apply: Desired(context, rollout.TargetImage, rollout.TargetPackage));
        }

        var versions = Append(context.Previous.Versions, new StageApplicationVersion
        {
            Version = rollout.TargetVersion,
            Image = rollout.TargetImage,
            Package = rollout.TargetPackage,
            RolledOutAt = Rfc3339.Format(context.Now),
            Outcome = StageRolloutOutcomes.RolledBack,
            Reason = reason
        });

        var back = new StageApplicationRollout
        {
            TargetVersion = lastGood.Version,
            TargetImage = lastGood.Image,
            TargetPackage = lastGood.Package,
            StartedAt = Rfc3339.Format(context.Now),
            IsRollback = true
        };

        return InProgress(context, back, versions);
    }

    static ReconcilePlan FailRollback(PlanningContext context, StageApplicationRollout rollout)
    {
        var reason = $"The rollback to version {rollout.TargetVersion} did not become ready within {context.Options.RolloutTimeout}.";
        var conditions = context.Conditions()
            .Set(ConditionTypes.Degraded, true, "RollbackFailed", reason)
            .Build();

        return new ReconcilePlan(
            context.Carry() with { Phase = StageApplicationPhases.Failed, Rollout = null, Conditions = conditions },
            Apply: Desired(context, rollout.TargetImage, rollout.TargetPackage));
    }

    static ReconcilePlan InProgress(PlanningContext context, StageApplicationRollout rollout, IReadOnlyList<StageApplicationVersion> versions)
    {
        var phase = PhaseOf(context, rollout);

        var conditions = context.Conditions()
            .Set(ConditionTypes.Progressing, true, rollout.IsRollback ? "RollingBack" : "RollingOut", $"Rolling {(rollout.IsRollback ? "back to" : "out")} version {rollout.TargetVersion}.");

        if (rollout.IsRollback)
        {
            conditions.Set(ConditionTypes.Degraded, true, "RollingBack", $"Version {context.Spec.Version} did not become ready.");
        }

        if (context.Previous.RunningVersion is not null)
        {
            conditions.Set(ConditionTypes.Ready, context.Observed.Deployment?.ReadyReplicas > 0, "Updating", $"Version {context.Previous.RunningVersion} keeps serving during the rollout.");
        }

        var status = context.Carry() with
        {
            Phase = phase,
            Rollout = rollout,
            Versions = versions,
            Conditions = conditions.Build()
        };

        return new ReconcilePlan(status, Apply: Desired(context, rollout.TargetImage, rollout.TargetPackage), RequeueAfter: _rolloutPoll);
    }

    static ReconcilePlan HoldAtLastGood(PlanningContext context, StageApplicationVersion? lastGood)
    {
        var last = context.Previous.Versions[^1];
        if (lastGood is null)
        {
            var failedConditions = context.Conditions()
                .Set(ConditionTypes.Degraded, true, "RolloutFailed", last.Reason ?? "The version did not become ready.")
                .Build();
            return new ReconcilePlan(
                context.Carry() with { Phase = StageApplicationPhases.Failed, Conditions = failedConditions },
                Apply: Desired(context, context.Spec.Image, PackageOf(context.Spec)));
        }

        var ready = context.Observed.Deployment?.IsRolledOutAt(lastGood.Image, lastGood.Package) == true;
        var conditions = context.Conditions()
            .Set(ConditionTypes.Ready, ready, ready ? "RolledBack" : "NotReady", $"Version {lastGood.Version} is running; version {context.Spec.Version} was rolled back.")
            .Set(ConditionTypes.Degraded, true, "RolledBack", last.Reason ?? "The requested version did not become ready and was rolled back.")
            .Build();

        return new ReconcilePlan(
            context.Carry() with { Phase = StageApplicationPhases.Running, RunningVersion = lastGood.Version, RunningImage = lastGood.Image, RunningPackage = lastGood.Package, Rollout = null, Conditions = conditions },
            Apply: Desired(context, lastGood.Image, lastGood.Package));
    }

    static ReconcilePlan Steady(PlanningContext context, string image, string version, string? package)
    {
        var ready = context.Observed.Deployment?.IsRolledOutAt(image, package) == true;
        var conditions = context.Conditions()
            .Set(ConditionTypes.Ready, ready, ready ? "RolledOut" : "NotReady", ready ? $"Version {version} is running." : $"Version {version} has no ready pod.")
            .Build();

        return new ReconcilePlan(
            context.Carry() with { Phase = StageApplicationPhases.Running, RunningVersion = version, RunningImage = image, RunningPackage = package, Rollout = null, Conditions = conditions },
            Apply: Desired(context, image, package),
            RequeueAfter: UntilExpiry(context));
    }

    static TimeSpan? UntilExpiry(PlanningContext context) => context.Until is { } until ? until - context.Now : null;

    static string? PackageOf(StageApplicationSpec spec) =>
        string.IsNullOrWhiteSpace(spec.Package?.ConfigMapRef) ? null : spec.Package.ConfigMapRef;

    static DesiredObjects Desired(PlanningContext context, string image, string? package) =>
        ApplicationObjectBuilder.Build(context.Application, image, 1, true, context.Options, package);

    static List<StageApplicationVersion> Append(IReadOnlyList<StageApplicationVersion> versions, StageApplicationVersion entry)
    {
        List<StageApplicationVersion> list = [.. versions, entry];
        return list.Count > 20 ? list[^20..] : list;
    }
}
