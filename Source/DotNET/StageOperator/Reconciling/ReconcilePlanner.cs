// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>
/// Decides what one reconcile does. A pure function of the resource, what the cluster holds and the time, so
/// every lifecycle decision - rollout, rollback, expiry, retention - is specified without a cluster.
/// </summary>
public static class ReconcilePlanner
{
    /// <summary>Plans a reconcile.</summary>
    /// <param name="context">The facts to decide from.</param>
    /// <returns>The plan.</returns>
    public static ReconcilePlan Plan(PlanningContext context)
    {
        if (context.Application.IsDeleting)
        {
            return PlanDeletion(context);
        }

        var problems = SpecValidator.Validate(context.Application);
        if (problems.Count > 0)
        {
            return PlanRefusal(context, problems);
        }

        var needsFinalizer = context.Application.Metadata.Finalizers?.Contains(StageApplication.RetentionFinalizer) != true;
        var plan = context.IsExpired ? PlanExpiry(context) : ActivePlanner.Plan(context);
        return plan with { EnsureFinalizer = needsFinalizer };
    }

    static ReconcilePlan PlanRefusal(PlanningContext context, IReadOnlyList<SpecProblem> problems)
    {
        var message = string.Join(' ', problems.Select(p => p.Message));
        var conditions = context.Conditions()
            .Set(ConditionTypes.Degraded, true, problems[0].Reason, message)
            .Build();

        return new ReconcilePlan(context.Carry() with { Phase = StageApplicationPhases.Failed, Conditions = conditions });
    }

    static ReconcilePlan PlanDeletion(PlanningContext context)
    {
        var deletedAt = context.Application.Metadata.DeletionTimestamp ?? context.Now;
        var retainedUntil = Rfc3339.Parse(context.Previous.DataRetainedUntil) ?? (deletedAt + context.Options.ExpiredDataRetention);
        var remaining = retainedUntil - context.Now;
        var conditions = context.Conditions()
            .Set(ConditionTypes.Degraded, true, "Deleting", $"The resource is being deleted; its data is kept until {Rfc3339.Format(retainedUntil)}.")
            .Build();
        var status = context.Carry() with
        {
            Phase = StageApplicationPhases.Stopped,
            Rollout = null,
            DataRetainedUntil = Rfc3339.Format(retainedUntil),
            Conditions = conditions
        };

        if (remaining <= TimeSpan.Zero)
        {
            return new ReconcilePlan(status, StopWorkloads: true, DeleteIngress: true, DeleteData: true, RemoveFinalizer: true);
        }

        return new ReconcilePlan(status, StopWorkloads: true, DeleteIngress: true, RequeueAfter: remaining);
    }

    static ReconcilePlan PlanExpiry(PlanningContext context)
    {
        var until = context.Until!.Value;
        var retainedUntil = until + context.Options.ExpiredDataRetention;
        var dataGone = !context.Observed.DataExists && context.Previous.Phase == StageApplicationPhases.Expired;
        var remaining = retainedUntil - context.Now;
        var removeData = remaining <= TimeSpan.Zero && context.Observed.DataExists;
        var message = remaining <= TimeSpan.Zero
            ? "The lifetime has ended and the data has been removed."
            : $"The lifetime ended at {Rfc3339.Format(until)}; the data is kept until {Rfc3339.Format(retainedUntil)}.";

        var conditions = context.Conditions()
            .Set(ConditionTypes.Expired, true, "LifetimeEnded", message)
            .Set(ConditionTypes.Degraded, false, "Expired", string.Empty)
            .Build();

        var status = context.Carry() with
        {
            Phase = StageApplicationPhases.Expired,
            Rollout = null,
            DataRetainedUntil = Rfc3339.Format(retainedUntil),
            Conditions = conditions
        };

        return new ReconcilePlan(
            status,
            StopWorkloads: !dataGone,
            DeleteIngress: !dataGone,
            DeleteData: removeData,
            RequeueAfter: remaining > TimeSpan.Zero ? remaining : null);
    }
}
