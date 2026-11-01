// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.Kubernetes;
using Cratis.Showrunner.StageOperator.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>Reconciles one application: observes the cluster, asks the planner what to do and does it.</summary>
public class Reconciler
{
    readonly IKubernetesGateway _gateway;
    readonly StageOperatorOptions _options;
    readonly TimeProvider _time;
    readonly ILogger<Reconciler> _logger;

    /// <summary>Initializes a new instance of the <see cref="Reconciler"/> class.</summary>
    /// <param name="gateway">The cluster.</param>
    /// <param name="options">The operator configuration.</param>
    /// <param name="time">The clock.</param>
    /// <param name="logger">The logger.</param>
    public Reconciler(IKubernetesGateway gateway, IOptions<StageOperatorOptions> options, TimeProvider time, ILogger<Reconciler> logger)
    {
        _gateway = gateway;
        _options = options.Value;
        _time = time;
        _logger = logger;
    }

    /// <summary>Reconciles an application.</summary>
    /// <param name="namespaceName">The namespace.</param>
    /// <param name="name">The name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>When to reconcile again before the next resync, or null.</returns>
    public async Task<TimeSpan?> Reconcile(string namespaceName, string name, CancellationToken cancellationToken)
    {
        var application = await _gateway.GetApplication(namespaceName, name, cancellationToken);
        if (application is null)
        {
            return null;
        }

        var observed = await _gateway.Observe(application, cancellationToken);
        var plan = ReconcilePlanner.Plan(new PlanningContext(application, observed, _time.GetUtcNow(), _options));

        // Order matters: the finalizer first, so data is never created that a deletion could miss; the
        // finalizer is released last, after the data it protected is gone.
        if (plan.EnsureFinalizer)
        {
            await _gateway.AddFinalizer(application, cancellationToken);
        }

        if (plan.StopWorkloads)
        {
            await _gateway.StopWorkloads(application, cancellationToken);
        }

        if (plan.DeleteIngress)
        {
            await _gateway.DeleteIngress(application, cancellationToken);
        }

        if (plan.Apply is not null)
        {
            await _gateway.Apply(plan.Apply, cancellationToken);
        }

        if (plan.DeleteData)
        {
            await _gateway.DeleteData(application, cancellationToken);
        }

        await _gateway.UpdateStatus(application, latest => Merge(plan.Status, latest), cancellationToken);

        if (plan.RemoveFinalizer)
        {
            await _gateway.RemoveFinalizer(application, cancellationToken);
        }

        if (plan.Status.Phase != (application.Status?.Phase ?? string.Empty))
        {
            _logger.LogInformation("{Namespace}/{Name} is now {Phase}", namespaceName, name, plan.Status.Phase);
        }

        return plan.RequeueAfter;
    }

    /// <summary>The usage fields belong to the sampler; whatever it wrote since the plan was made is kept.</summary>
    /// <param name="planned">The status the plan decided.</param>
    /// <param name="latest">The status as it is now.</param>
    /// <returns>The planned status with the latest usage.</returns>
    static StageApplicationStatus Merge(StageApplicationStatus planned, StageApplicationStatus latest) => planned with
    {
        ResourceUsage = latest.ResourceUsage,
        LastPeriodUsage = latest.LastPeriodUsage,
        CurrentDayUsage = latest.CurrentDayUsage
    };
}
