// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.Kubernetes;
using Cratis.Showrunner.StageOperator.Metering;
using Cratis.Showrunner.StageOperator.Resources;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.StageOperator.Hosting;

/// <summary>Samples the pod metrics of every running application and accumulates them into the status.</summary>
public class UsageSamplerService : BackgroundService
{
    static readonly HashSet<string> _measuredPhases =
    [
        StageApplicationPhases.Pending,
        StageApplicationPhases.Deploying,
        StageApplicationPhases.Running,
        StageApplicationPhases.RollingBack
    ];

    readonly IKubernetesGateway _gateway;
    readonly StageOperatorOptions _options;
    readonly TimeProvider _time;
    readonly ILogger<UsageSamplerService> _logger;

    /// <summary>Initializes a new instance of the <see cref="UsageSamplerService"/> class.</summary>
    /// <param name="gateway">The cluster.</param>
    /// <param name="options">The operator configuration.</param>
    /// <param name="time">The clock.</param>
    /// <param name="logger">The logger.</param>
    public UsageSamplerService(IKubernetesGateway gateway, IOptions<StageOperatorOptions> options, TimeProvider time, ILogger<UsageSamplerService> logger)
    {
        _gateway = gateway;
        _options = options.Value;
        _time = time;
        _logger = logger;
    }

    /// <summary>Takes one sample of every running application.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when every application is sampled.</returns>
    public async Task Sample(CancellationToken cancellationToken)
    {
        foreach (var namespaceName in WatchScope.Namespaces(_options))
        {
            foreach (var application in await _gateway.ListApplications(namespaceName, cancellationToken))
            {
                if (application.IsDeleting || !_measuredPhases.Contains(application.Status?.Phase ?? string.Empty))
                {
                    continue;
                }

                try
                {
                    var usage = await _gateway.GetContainerUsage(application, cancellationToken);
                    if (usage is not { Count: > 0 })
                    {
                        continue;
                    }

                    var now = _time.GetUtcNow();
                    await _gateway.UpdateStatus(application, status => UsageAccumulator.Add(status, usage, now), cancellationToken);
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning(ex, "Sampling {Namespace}/{Name} failed", application.Namespace, application.Name);
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.MetricsSampleInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await Sample(stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(ex, "Sampling resource usage failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutting down.
        }
    }
}
