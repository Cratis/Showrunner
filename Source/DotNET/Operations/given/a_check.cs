// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// A check run against a fake cluster on a manual clock.
/// </summary>
public abstract class a_check : Specification
{
    protected ManualTimeProvider _time;
    protected FakeClusterReader _reader;
    protected IReadOnlyCollection<Finding> _findings;

    /// <summary>
    /// Gets the keys of the findings.
    /// </summary>
    protected IEnumerable<string> Keys => _findings.Select(finding => finding.Key);

    /// <summary>
    /// Gets the check under specification.
    /// </summary>
    protected abstract IWatchdogCheck Check { get; }

    void Establish()
    {
        _time = new ManualTimeProvider();
        _reader = new FakeClusterReader();
    }

    protected async Task Run(TimeSpan? window = null, TimeSpan? budget = null) =>
        _findings = await Check.Collect(CheckContexts.At(_time, window, budget), CancellationToken.None);
}
#endif
