// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Runs the remedies Direct's configuration defines, only as governed. While Direct answers, a failing verification check
/// asks Direct (a request, idempotent by identity), and only an authorization Direct returns - re-checked here against revision,
/// epoch, expiry, target state, budget and cooldown - is executed. While Direct is unreachable the offline policy decides, and
/// every uncertainty denies. A remedy that does nothing is the default: without a local binding (or enabled HTTP remedies) a
/// remedy is denied, never guessed at.
/// </summary>
/// <param name="options">The link options.</param>
/// <param name="client">The Direct client.</param>
/// <param name="store">The state store.</param>
/// <param name="remedies">The remedies this watchdog can execute, by the name of the Direct remedy.</param>
/// <param name="fallback">Creates the executor of a remedy without a local binding, or <see langword="null"/> when there is none.</param>
/// <param name="timeProvider">The clock.</param>
internal sealed class LinkRemedies(
    DirectLinkOptions options,
    IDirectClient client,
    ILinkStore store,
    IReadOnlyDictionary<string, IRemedy> remedies,
    Func<string, IRemedy>? fallback,
    TimeProvider timeProvider)
{
    const int RememberedHandled = 200;

    /// <summary>
    /// Runs the remedies the observations call for.
    /// </summary>
    /// <param name="context">The pass.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the remedies of the pass have run.</returns>
    internal async Task Run(LinkContext context, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        if (context.Active(now) is not { } configuration || context.Fenced || !context.Enrolled)
        {
            return;
        }

        Escalate(context, configuration.Snapshot, now);
        if (context.Connected)
        {
            await Online(context, configuration, now, cancellationToken);
        }
        else if (context.Connection == LinkConnection.Offline)
        {
            await Offline(context, configuration, now, cancellationToken);
        }
    }

    static Guid RequestIdentity(string instance, RemedyDefinition remedy, CheckObservation observation, int attempts)
    {
        var text = $"{instance}:{remedy.Name}:{observation.FirstFailedAt.UtcTicks}:{attempts}";
        return new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(text))[..16]);
    }

    static ObservedCheckState StateOf(LinkContext context, string check) =>
        context.Memory.Observations.TryGetValue(check, out var observation) ? observation.State : ObservedCheckState.Unknown;

    static int InWindow(IReadOnlyList<DateTimeOffset> attempts, DateTimeOffset now) =>
        attempts.Count(started => now - started < OfflineRemedyPolicy.AttemptWindow);

    IReadOnlyList<DateTimeOffset> AttemptsOf(LinkContext context, string remedy) =>
        context.Memory.Attempts.TryGetValue(remedy, out var attempts) ? attempts : [];

    bool Handled(LinkContext context, string key) => context.Memory.Handled.Contains(key);

    void Handle(LinkContext context, string key) =>
        context.Memory = context.Memory with { Handled = [.. context.Memory.Handled.Append(key).TakeLast(RememberedHandled)] };

    void Escalate(LinkContext context, WatchdogConfigurationSnapshot configuration, DateTimeOffset now)
    {
        foreach (var remedy in configuration.Remedies)
        {
            var attempts = AttemptsOf(context, remedy.Name);
            if (StateOf(context, remedy.Verification) == ObservedCheckState.Failing && attempts.Count > 0 &&
                InWindow(attempts, now) >= remedy.MaxAttempts && !context.Memory.Escalated.Contains(remedy.Name))
            {
                context.Outbox.Enqueue(new RemedyEscalated(remedy.Name, remedy.Resource, attempts.Count, "the attempt budget is spent and the verification check is still failing"));
                context.Memory = context.Memory with { Escalated = [.. context.Memory.Escalated, remedy.Name] };
            }
        }
    }

    async Task Online(LinkContext context, SignedWatchdogConfiguration configuration, DateTimeOffset now, CancellationToken cancellationToken)
    {
        foreach (var remedy in configuration.Snapshot.Remedies.Where(remedy => remedy.OnlineMode != RemedyPolicyMode.Never))
        {
            if (!context.Memory.Observations.TryGetValue(remedy.Verification, out var observation) || observation.State != ObservedCheckState.Failing || observation.EvidenceEvent == Guid.Empty)
            {
                continue;
            }

            var attempts = InWindow(AttemptsOf(context, remedy.Name), now);
            var request = RequestIdentity(options.InstanceName, remedy, observation, attempts);
            if (Handled(context, $"request:{request}"))
            {
                continue;
            }

            var answer = await client.RemedyRequest(
                context.Credentials,
                new WatchdogRemedyRequest(request, remedy.Name, configuration.Snapshot.Revision, observation.EvidenceEvent, observation.ObservedAt),
                cancellationToken);
            if (!context.Succeeded(answer, "requesting a remedy"))
            {
                return;
            }

            Handle(context, $"request:{request}");
        }

        await Authorized(context, configuration, now, cancellationToken);
    }

    async Task Authorized(LinkContext context, SignedWatchdogConfiguration configuration, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var answer = await client.RemedyAuthorizations(context.Credentials, cancellationToken);
        if (!context.Succeeded(answer, "reading remedy authorizations") || answer.Value is null)
        {
            return;
        }

        foreach (var authorization in answer.Value.Authorized.Where(authorization => !Handled(context, $"authorization:{authorization.Request}")))
        {
            Handle(context, $"authorization:{authorization.Request}");
            var remedy = configuration.Snapshot.Remedies.FirstOrDefault(candidate => candidate.Name == authorization.Remedy);
            var problem = AuthorizationRecheck.Problem(
                authorization,
                configuration.Snapshot,
                context.Identity!.Epoch,
                context.Connected,
                remedy is null ? ObservedCheckState.Unknown : StateOf(context, remedy.Verification),
                remedy is null ? [] : AttemptsOf(context, remedy.Name),
                now);
            if (problem.Length > 0 || remedy is null)
            {
                Deny(context, authorization.Remedy, authorization.Resource, AssuranceOutcome.Block, "AuthorizationRecheck", problem);
                continue;
            }

            await Execute(context, configuration.Snapshot, remedy, false, authorization.Request, cancellationToken);
        }
    }

    async Task Offline(LinkContext context, SignedWatchdogConfiguration configuration, DateTimeOffset now, CancellationToken cancellationToken)
    {
        // The epoch can only be trusted offline while the last confirmed claim is within its lease.
        var certain = context.Identity!.ConfirmedAt is { } confirmed && now - confirmed < WatchdogProtocol.LeaseDuration;
        foreach (var remedy in configuration.Snapshot.Remedies)
        {
            if (!context.Memory.Observations.TryGetValue(remedy.Verification, out var observation) || observation.State != ObservedCheckState.Failing)
            {
                continue;
            }

            var decision = OfflineRemedyPolicy.Evaluate(new OfflineRemedyContext(
                configuration,
                context.Identity.SigningKey,
                options.InstanceName,
                remedy.Name,
                now,
                true,
                certain,
                ObservedCheckState.Failing,
                observation.ObservedAt,
                AttemptsOf(context, remedy.Name)));
            if (decision.IsPermitted)
            {
                await Execute(context, configuration.Snapshot, remedy, true, Guid.Empty, cancellationToken);
            }
            else if (!Handled(context, $"denied:{remedy.Name}:{observation.FirstFailedAt.UtcTicks}:{decision.Reason}"))
            {
                Handle(context, $"denied:{remedy.Name}:{observation.FirstFailedAt.UtcTicks}:{decision.Reason}");
                Deny(context, remedy.Name, remedy.Resource, decision.Outcome, decision.Reason.ToString(), decision.Explanation);
            }
        }
    }

    IRemedy? ExecutorOf(string name) => remedies.TryGetValue(name, out var bound) ? bound : fallback?.Invoke(name);

    void Deny(LinkContext context, string remedy, Guid resource, AssuranceOutcome outcome, string reason, string explanation)
    {
        context.Outbox.Enqueue(new RemedyDenied(remedy, resource, outcome.ToString(), reason, explanation));
        context.RemedyActions.Add($"{remedy}: denied ({reason})");
    }

    async Task Execute(LinkContext context, WatchdogConfigurationSnapshot configuration, RemedyDefinition remedy, bool offline, Guid authorization, CancellationToken cancellationToken)
    {
        if (ExecutorOf(remedy.Name) is not { } executor)
        {
            Deny(context, remedy.Name, remedy.Resource, AssuranceOutcome.Block, "NoExecutor", $"'{remedy.Name}' has no local binding and HTTP remedies are not enabled");
            return;
        }

        var now = timeProvider.GetUtcNow();
        var attempts = AttemptsOf(context, remedy.Name).Where(started => now - started < OfflineRemedyPolicy.AttemptWindow).Append(now).ToList();

        // The attempt is recorded before it runs: a crash must never make the budget forget it.
        context.Memory = context.Memory with { Attempts = new Dictionary<string, IReadOnlyList<DateTimeOffset>>(context.Memory.Attempts) { [remedy.Name] = attempts } };
        await store.SaveMemory(context.Memory, cancellationToken);
        context.Outbox.Enqueue(new RemedyAttempted(remedy.Name, remedy.Resource, attempts.Count, offline, authorization, configuration.Revision, context.Identity!.Epoch));
        await store.SaveOutbox(context.Outbox.ToSnapshot(), cancellationToken);

        RemedyResult result;
        try
        {
            result = await executor.Execute(new RemedyInvocation(remedy, configuration.Secrets, offline), cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            result = new RemedyResult(false, 0, $"the remedy failed: {error.GetType().Name}");
        }

        if (result.Succeeded)
        {
            context.Outbox.Enqueue(new RemedySucceeded(remedy.Name, remedy.Resource, result.StatusCode, result.Detail));
            context.Memory = context.Memory with { AwaitingVerification = new Dictionary<string, DateTimeOffset>(context.Memory.AwaitingVerification) { [remedy.Name] = now } };
        }
        else
        {
            context.Outbox.Enqueue(new RemedyFailed(remedy.Name, remedy.Resource, result.StatusCode, result.Detail));
        }

        context.RemedyActions.Add($"{remedy.Name}: {(result.Succeeded ? "ran" : "failed")}{(offline ? " (offline)" : string.Empty)} - {result.Detail}");
    }
}
