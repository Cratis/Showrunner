// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Globalization;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Asks Chronicle for the current state of its observers, the same query API the Workbench uses: partitions that ran out of retries,
/// partitions still being retried after a quarantine-worthy time, and quarantined observers. Reading state rather than counting metric
/// increases means a partition that stays stuck stays a finding. Whatever cannot be read becomes a finding of its own and never hides what could.
/// Partition and observer state has already persisted for a while, so a successful read is kept for the confirmation re-check.
/// </summary>
/// <param name="chronicle">The Chronicle reader.</param>
/// <param name="options">The options.</param>
public sealed class ChronicleObserversCheck(IChronicleReader chronicle, ChronicleObserversOptions options) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "chronicle-observers";

    const int Parallelism = 8;

    static readonly TimeSpan _budget = TimeSpan.FromSeconds(45);
    static readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(10);

    IReadOnlyCollection<Finding>? _kept;

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        // A failure to read may be a blip, so it is never kept; the re-check reads Chronicle again.
        if (_kept is not null)
        {
            return _kept;
        }

        var findings = await Read(context, cancellationToken);
        if (!findings.Any(finding => finding.Key == "chronicle:api"))
        {
            _kept = findings;
        }

        return findings;
    }

    static string Since(IEnumerable<ChronicleFailedPartition> partitions)
    {
        var first = partitions.Select(partition => partition.FirstFailure).Where(time => time is not null).Select(time => time!.Value).Order().Cast<DateTimeOffset?>().FirstOrDefault();
        return first is { } time ? time.UtcDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " UTC" : "an unknown time";
    }

    async Task<IReadOnlyCollection<Finding>> Read(CheckContext context, CancellationToken cancellationToken)
    {
        var now = context.TimeProvider.GetUtcNow();
        var deadline = now + _budget < context.CollectionDeadline ? now + _budget : context.CollectionDeadline;
        TimeSpan Timeout()
        {
            var remaining = deadline - context.TimeProvider.GetUtcNow();
            if (remaining <= TimeSpan.Zero)
            {
                throw new TimeoutException($"not finished within {(int)_budget.TotalSeconds}s");
            }

            return remaining < _requestTimeout ? remaining : _requestTimeout;
        }

        IReadOnlyList<string> stores;
        try
        {
            stores = await chronicle.EventStores(Timeout(), cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            // Not being able to look is the finding, never "healthy".
            return [new Finding("chronicle:api", $"could not read Chronicle's observer state: {Redaction.Summarize(error.Message)}")];
        }

        var findings = new ConcurrentBag<Finding>();
        var unreadable = new ConcurrentQueue<string>();
        using var gate = new SemaphoreSlim(Parallelism);

        // Every namespace of every store is read, and a store's namespaces are as many as its tenants; read side by side.
        async Task ReadNamespace(string store, string @namespace)
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                var failed = await chronicle.FailedPartitions(store, @namespace, Timeout(), cancellationToken);
                var observers = await chronicle.Observers(store, @namespace, Timeout(), cancellationToken);
                foreach (var finding in FindingsFor(store, @namespace, failed, observers, now))
                {
                    findings.Add(finding);
                }
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                unreadable.Enqueue($"{store}/{@namespace}: {Redaction.Summarize(error.Message)}");
            }
            finally
            {
                gate.Release();
            }
        }

        var reads = new List<Task>();
        foreach (var store in stores)
        {
            try
            {
                var namespaces = await chronicle.Namespaces(store, Timeout(), cancellationToken);
                reads.AddRange(namespaces.Select(@namespace => ReadNamespace(store, @namespace)));
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                unreadable.Enqueue($"{store}: {Redaction.Summarize(error.Message)}");
            }
        }

        await Task.WhenAll(reads);
        var result = findings.ToDictionary(finding => finding.Key, StringComparer.Ordinal);
        if (!unreadable.IsEmpty)
        {
            var reasons = unreadable.Order(StringComparer.Ordinal).ToList();
            result["chronicle:api"] = new Finding(
                "chronicle:api",
                $"could not read Chronicle's observer state for {reasons.Count} event store(s) or namespace(s): {string.Join("; ", reasons.Take(5))}");
        }

        return [.. result.Values];
    }

    IEnumerable<Finding> FindingsFor(string store, string @namespace, IReadOnlyList<ChronicleFailedPartition> failedPartitions, IReadOnlyList<ChronicleObserver> observers, DateTimeOffset now)
    {
        var inspect = $"Inspect with `cratis chronicle failed-partitions list -e {store} -n {@namespace}` or the Workbench";
        foreach (var group in failedPartitions.Where(partition => !partition.IsResolved).GroupBy(partition => partition.ObserverId).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var quarantined = group.Where(partition => partition.IsQuarantined).ToList();
            if (quarantined.Count > 0)
            {
                var text = $"Chronicle observer {group.Key} in {store}/{@namespace}: {quarantined.Count} partition(s) ran out of retries, failing since {Since(quarantined)}. "
                    + $"Chronicle will not retry them until they are cleared or replayed. {inspect}";
                yield return new Finding($"chronicle:{store}/{@namespace}/{group.Key}:PartitionsQuarantined", text);
            }

            var failing = group
                .Where(partition => !partition.IsQuarantined && partition.FirstFailure is { } first && now - first >= options.FailingFor)
                .ToList();
            if (failing.Count > 0)
            {
                yield return new Finding(
                    $"chronicle:{store}/{@namespace}/{group.Key}:PartitionsFailing",
                    $"Chronicle observer {group.Key} in {store}/{@namespace}: {failing.Count} partition(s) still being retried, failing since {Since(failing)}. {inspect}");
            }
        }

        foreach (var observer in observers.Where(observer => observer.IsQuarantined))
        {
            var text = $"Chronicle observer {observer.Id} in {store}/{@namespace} is quarantined and processes no events until it is cleared "
                + "(`cratis chronicle observers clear-quarantine`) or its client subscribes again";
            yield return new Finding($"chronicle:{store}/{@namespace}/{observer.Id}:ObserverQuarantined", text);
        }
    }
}
