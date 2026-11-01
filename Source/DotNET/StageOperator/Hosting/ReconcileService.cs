// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Channels;
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.Kubernetes;
using Cratis.Showrunner.StageOperator.Reconciling;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.StageOperator.Hosting;

/// <summary>
/// Runs the reconcile loop: watches the <c>StageApplication</c> resources, re-lists them every resync interval so a
/// missed event is only ever late, and reconciles one resource at a time from a de-duplicated queue.
/// </summary>
public class ReconcileService : BackgroundService
{
    static readonly TimeSpan _watchRestartDelay = TimeSpan.FromSeconds(5);
    static readonly TimeSpan _failureRetryDelay = TimeSpan.FromSeconds(15);

    readonly IKubernetesGateway _gateway;
    readonly Reconciler _reconciler;
    readonly StageOperatorOptions _options;
    readonly ILogger<ReconcileService> _logger;
    readonly Channel<(string Namespace, string Name)> _queue = Channel.CreateUnbounded<(string, string)>();
    readonly HashSet<(string Namespace, string Name)> _queued = [];
    readonly Lock _lock = new();

    /// <summary>Initializes a new instance of the <see cref="ReconcileService"/> class.</summary>
    /// <param name="gateway">The cluster.</param>
    /// <param name="reconciler">The reconciler.</param>
    /// <param name="options">The operator configuration.</param>
    /// <param name="logger">The logger.</param>
    public ReconcileService(IKubernetesGateway gateway, Reconciler reconciler, IOptions<StageOperatorOptions> options, ILogger<ReconcileService> logger)
    {
        _gateway = gateway;
        _reconciler = reconciler;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task> { Resync(stoppingToken), Work(stoppingToken) };
        tasks.AddRange(WatchScope.Namespaces(_options).Select(ns => Watch(ns, stoppingToken)));
        return Task.WhenAll(tasks);
    }

    static async Task Delay(TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Shutting down.
        }
    }

    void Enqueue(string namespaceName, string name)
    {
        lock (_lock)
        {
            if (!_queued.Add((namespaceName, name)))
            {
                return;
            }
        }

        _queue.Writer.TryWrite((namespaceName, name));
    }

    async Task Watch(string? namespaceName, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await foreach (var application in _gateway.WatchApplications(namespaceName, cancellationToken))
                {
                    Enqueue(application.Namespace, application.Name);
                }
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "The watch of {Namespace} ended; restarting", namespaceName ?? "all namespaces");
            }

            await Delay(_watchRestartDelay, cancellationToken);
        }
    }

    async Task Resync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var namespaceName in WatchScope.Namespaces(_options))
            {
                try
                {
                    foreach (var application in await _gateway.ListApplications(namespaceName, cancellationToken))
                    {
                        Enqueue(application.Namespace, application.Name);
                    }
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning(ex, "Listing the applications of {Namespace} failed", namespaceName ?? "all namespaces");
                }
            }

            await Delay(_options.ResyncInterval, cancellationToken);
        }
    }

    async Task Work(CancellationToken cancellationToken)
    {
        await foreach (var (namespaceName, name) in _queue.Reader.ReadAllAsync(cancellationToken))
        {
            lock (_lock)
            {
                _queued.Remove((namespaceName, name));
            }

            try
            {
                var again = await _reconciler.Reconcile(namespaceName, name, cancellationToken);
                if (again is { } after)
                {
                    _ = Requeue(namespaceName, name, after, cancellationToken);
                }
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Reconciling {Namespace}/{Name} failed", namespaceName, name);
                _ = Requeue(namespaceName, name, _failureRetryDelay, cancellationToken);
            }
        }
    }

    async Task Requeue(string namespaceName, string name, TimeSpan after, CancellationToken cancellationToken)
    {
        await Delay(after < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1) : after, cancellationToken);
        if (!cancellationToken.IsCancellationRequested)
        {
            Enqueue(namespaceName, name);
        }
    }
}
