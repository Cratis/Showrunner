// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// Decides whether a remedy may run while Direct is unreachable. The default is to observe and queue; an exact remedy
/// runs only when a signed, unexpired snapshot grants it Automatic offline mode, the queue is durable, fencing is certain,
/// a verification check exists, the target is known to be failing on fresh evidence, and the attempt budget and cooldown
/// pass. Review is never satisfiable offline. Every uncertainty denies.
/// </summary>
public static class OfflineRemedyPolicy
{
    /// <summary>
    /// The rolling window a remedy's <see cref="RemedyDefinition.MaxAttempts"/> is counted over.
    /// </summary>
    public static readonly TimeSpan AttemptWindow = TimeSpan.FromHours(24);

    /// <summary>
    /// Evaluates the offline decision.
    /// </summary>
    /// <param name="context">What the decision is made from.</param>
    /// <returns>The decision.</returns>
    public static OfflineRemedyDecision Evaluate(OfflineRemedyContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var configuration = context.Configuration;
        if (configuration?.Snapshot is null || !ConfigurationSignature.Verify(configuration, context.SigningKey))
        {
            return Block(OfflineRemedyReason.UnsignedConfiguration, "There is no configuration signed for this instance");
        }

        var snapshot = configuration.Snapshot;
        if (!string.Equals(snapshot.InstanceName, context.InstanceName, StringComparison.Ordinal))
        {
            return Block(OfflineRemedyReason.WrongInstance, $"The configuration was issued for '{snapshot.InstanceName}'");
        }

        if (snapshot.ExpiresAt <= context.Now)
        {
            return Block(OfflineRemedyReason.ExpiredConfiguration, $"The configuration expired at {snapshot.ExpiresAt:O}");
        }

        var remedy = snapshot.Remedies?.FirstOrDefault(candidate => string.Equals(candidate.Name, context.Remedy, StringComparison.Ordinal));
        if (remedy is null)
        {
            return Block(OfflineRemedyReason.UnknownRemedy, $"The configuration has no remedy '{context.Remedy}'");
        }

        return ByPolicy(snapshot, remedy) ?? ByRuntime(context) ?? ByTarget(snapshot, remedy, context) ?? ByBudget(remedy, context) ??
            new OfflineRemedyDecision(AssuranceOutcome.Allow, OfflineRemedyReason.Permitted, $"'{remedy.Name}' may run once under revision {snapshot.Revision}");
    }

    static OfflineRemedyDecision? ByPolicy(WatchdogConfigurationSnapshot snapshot, RemedyDefinition remedy)
    {
        if (snapshot.Offline is not { RemediesPermitted: true })
        {
            return Block(OfflineRemedyReason.OfflineRemediesDisabled, "The configuration permits no remedy offline - observing and queuing only");
        }

        return remedy.OfflineMode switch
        {
            RemedyPolicyMode.Automatic => null,
            RemedyPolicyMode.Review => new(AssuranceOutcome.RequiresHumanReview, OfflineRemedyReason.ReviewRequired, $"'{remedy.Name}' needs a person's review, which cannot happen while Direct is unreachable"),
            _ => Block(OfflineRemedyReason.ObserveOnly, $"'{remedy.Name}' never runs offline - observing and queuing only"),
        };
    }

    static OfflineRemedyDecision? ByRuntime(OfflineRemedyContext context)
    {
        if (!context.DurableQueue)
        {
            return Block(OfflineRemedyReason.NoDurableQueue, "The outbox is not durable, so an attempt could not be recorded and replayed");
        }

        return context.FencingCertain
            ? null
            : Block(OfflineRemedyReason.FencingUncertain, "This instance cannot be sure it still holds its fencing epoch");
    }

    static OfflineRemedyDecision? ByTarget(WatchdogConfigurationSnapshot snapshot, RemedyDefinition remedy, OfflineRemedyContext context)
    {
        var verified = !string.IsNullOrWhiteSpace(remedy.Verification) &&
            (snapshot.Checks ?? []).Any(check => string.Equals(check.Name, remedy.Verification, StringComparison.Ordinal));
        if (!verified)
        {
            return Block(OfflineRemedyReason.MissingVerification, $"'{remedy.Name}' has no verification check in this configuration");
        }

        return context.TargetState switch
        {
            ObservedCheckState.Failing => ByFreshness(remedy, context),
            ObservedCheckState.Passing => Block(OfflineRemedyReason.NothingToRemedy, $"'{remedy.Verification}' is passing - there is nothing to remedy"),
            _ => Block(OfflineRemedyReason.UnknownState, $"The state of '{remedy.Verification}' is unknown"),
        };
    }

    static OfflineRemedyDecision? ByFreshness(RemedyDefinition remedy, OfflineRemedyContext context)
    {
        var age = context.Now - context.EvidenceObservedAt;
        return age < TimeSpan.Zero || remedy.EvidenceFreshness <= TimeSpan.Zero || age > remedy.EvidenceFreshness
            ? Block(OfflineRemedyReason.StaleEvidence, $"The failing observation is {age} old; '{remedy.Name}' needs one within {remedy.EvidenceFreshness}")
            : null;
    }

    static OfflineRemedyDecision? ByBudget(RemedyDefinition remedy, OfflineRemedyContext context)
    {
        // An attempt stamped in the future is counted, not ignored: a clock that moved backwards must not reopen the budget.
        var attempts = (context.PreviousAttempts ?? []).ToArray();
        var inWindow = attempts.Count(attempt => context.Now - attempt < AttemptWindow);
        if (remedy.MaxAttempts <= 0 || inWindow >= remedy.MaxAttempts)
        {
            return Block(OfflineRemedyReason.AttemptBudgetExhausted, $"'{remedy.Name}' was attempted {inWindow} time(s) in {AttemptWindow}; the budget is {remedy.MaxAttempts}");
        }

        var last = attempts.Length == 0 ? (DateTimeOffset?)null : attempts.Max();
        return last is { } previous && context.Now - previous < remedy.Cooldown
            ? Block(OfflineRemedyReason.CoolingDown, $"'{remedy.Name}' was attempted at {previous:O}; the cooldown is {remedy.Cooldown}")
            : null;
    }

    static OfflineRemedyDecision Block(OfflineRemedyReason reason, string explanation) => new(AssuranceOutcome.Block, reason, explanation);
}
