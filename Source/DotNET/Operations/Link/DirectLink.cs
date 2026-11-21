// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// The optional link between a watchdog pass and Cratis Direct's watchdog protocol, adapted to a watchdog that runs as a short-lived
/// Kubernetes CronJob: everything a long-running watchdog keeps in memory - the credential, the epoch, the held configuration,
/// the outbox, the observations - is loaded at the start of a pass and saved before it ends.
/// </summary>
/// <remarks>
/// <para>Local configuration always applies and remains the fallback: Direct adds its endpoint checks and governs its remedies on
/// top, and nothing the link does can stop the local checks or the alert channels.</para>
/// <para>A pass: <see cref="Begin"/> claims (with the enrollment token once, with the credential after), syncs and acknowledges
/// the signed configuration, and delivers the outbox; the checks run; <see cref="Observe"/> turns what they found into facts and
/// runs the remedies Direct governs; <see cref="End"/> queues a heartbeat, delivers the outbox again and saves the state and status.</para>
/// </remarks>
/// <param name="options">The link options.</param>
/// <param name="client">The client of Direct's watchdog protocol.</param>
/// <param name="store">Where the link keeps its state between passes.</param>
/// <param name="remedies">The remedies this watchdog can execute, each by the name of the Direct remedy it carries out.</param>
/// <param name="fallback">Creates the executor of a remedy without a local binding, or <see langword="null"/> when there is none.</param>
/// <param name="timeProvider">The clock.</param>
/// <param name="logger">The logger.</param>
public sealed class DirectLink(
    DirectLinkOptions options,
    IDirectClient client,
    ILinkStore store,
    IEnumerable<IRemedy> remedies,
    Func<string, IRemedy>? fallback,
    TimeProvider timeProvider,
    ILogger<DirectLink> logger) : IWatchdogLink
{
    readonly LinkClaim _claim = new(options, client, store, timeProvider);
    readonly LinkConfiguration _configuration = new(options, client, store, timeProvider, logger);
    readonly LinkDelivery _delivery = new(client, store);
    readonly LinkObservations _observations = new(options, timeProvider);
    readonly LinkRemedies _remedies = new(options, client, store, remedies.ToDictionary(remedy => remedy.Name, StringComparer.Ordinal), fallback, timeProvider);
    readonly Dictionary<string, EndpointCheckResult> _endpointResults = new(StringComparer.Ordinal);
    LinkContext _context = new();

    /// <summary>
    /// Gets the configuration that may be acted on now - held, signed under the pinned key and unexpired - or <see langword="null"/>.
    /// </summary>
    public WatchdogConfigurationSnapshot? ActiveConfiguration => _context.Active(timeProvider.GetUtcNow())?.Snapshot;

    /// <summary>
    /// Records what one of Direct's endpoint checks found in this pass.
    /// </summary>
    /// <param name="result">The result.</param>
    public void RecordEndpoint(EndpointCheckResult result) => _endpointResults[result.Name] = result;

    /// <inheritdoc/>
    public async Task Begin(CancellationToken cancellationToken)
    {
        _context = new LinkContext();
        _endpointResults.Clear();
        try
        {
            await Load(cancellationToken);
            if (await _claim.Claim(_context, cancellationToken))
            {
                await _configuration.Synchronize(_context, cancellationToken);
                await _delivery.Flush(_context, cancellationToken);
            }
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            Refuse($"the link could not start: {error.GetType().Name}");
        }
    }

    /// <inheritdoc/>
    public async Task Observe(IReadOnlyDictionary<string, Finding> findings, CancellationToken cancellationToken)
    {
        if (!_context.Ready || !_context.Enrolled || _context.Fenced)
        {
            return;
        }

        try
        {
            _observations.Record(_context, findings, [.. _endpointResults.Values]);
            await _remedies.Run(_context, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _context.Problems.Add($"recording the pass failed: {error.GetType().Name}");
        }
    }

    /// <inheritdoc/>
    public async Task End(CancellationToken cancellationToken)
    {
        if (_context.Ready)
        {
            await Finish(cancellationToken);
        }

        if (_context.Identity?.Held?.Snapshot is { } held && held.ExpiresAt <= timeProvider.GetUtcNow())
        {
            _context.Problems.Add($"the configuration held (revision {held.Revision}) lapsed at {held.ExpiresAt:O}; only local configuration applies");
        }

        var status = StatusOf();
        if (_context.Problems.Count > 0 || _context.Connection != LinkConnection.Connected)
        {
            logger.Degraded(status.Summary());
        }
        else
        {
            logger.Status(status.Summary());
        }

        try
        {
            await store.SaveStatus(status, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            logger.StateNotSaved(error.GetType().Name);
        }
    }

    async Task Load(CancellationToken cancellationToken)
    {
        try
        {
            _context.Identity = await store.LoadIdentity(cancellationToken);
            _context.Outbox = new Outbox(options.InstanceName, await store.LoadOutbox(cancellationToken), options.OutboxLimit, timeProvider);
            _context.Memory = await store.LoadMemory(cancellationToken);
            _context.Ready = true;
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            // Without the saved credential a claim would be a first claim again: never go on with a state that could not be read.
            _context.Ready = false;
            throw new InvalidOperationException($"the saved link state could not be read ({error.GetType().Name})", error);
        }
    }

    async Task Finish(CancellationToken cancellationToken)
    {
        try
        {
            if (_context.Enrolled && !_context.Fenced && !_context.Outbox.HasPending(WatchdogEventTypes.Heartbeat))
            {
                var applied = _context.Active(timeProvider.GetUtcNow())?.Snapshot.Revision ?? 0;
                _context.Outbox.Enqueue(new Heartbeat(applied, options.RuntimeVersion, true, _context.Outbox.Depth));
            }

            await _delivery.Flush(_context, cancellationToken);
            await store.SaveMemory(_context.Memory, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _context.Problems.Add($"the link state could not be saved: {error.GetType().Name}");
        }
    }

    void Refuse(string problem)
    {
        _context.Connection = LinkConnection.Refused;
        _context.Problems.Add(problem);
    }

    LinkStatus StatusOf()
    {
        var now = timeProvider.GetUtcNow();
        var held = _context.Identity?.Held?.Snapshot;
        return new LinkStatus(
            options.InstanceName,
            _context.Connection,
            now,
            _context.Identity?.Epoch ?? 0,
            held?.Revision ?? 0,
            held?.ExpiresAt,
            _context.Ready ? _context.Outbox.Depth : 0,
            _context.Ready ? _context.Outbox.Watermark : 0,
            _context.Ready ? _context.Outbox.Evicted : 0,
            _context.Delivered,
            [.. _context.RemedyActions],
            [.. _context.Problems]);
    }
}
