// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Probe;

/// <summary>
/// What a probe found. A request that fails is asked again once after a pause, and only a request that fails both times is down:
/// one that fails and then answers is flaky and does not alert.
/// </summary>
/// <param name="Rounds">The results of every round: all requests first, then only the ones that failed.</param>
/// <param name="Failing">The requests that failed both rounds.</param>
/// <param name="Flaky">The requests that failed the first round and answered the second.</param>
public sealed record ProbeReport(IReadOnlyList<IReadOnlyList<ProbeResult>> Rounds, IReadOnlyList<ProbeResult> Failing, IReadOnlyList<ProbeResult> Flaky)
{
    /// <summary>
    /// Gets a value indicating whether something is down.
    /// </summary>
    public bool IsDown => Failing.Count > 0;

    /// <summary>
    /// Gets the failing requests, one per line.
    /// </summary>
    public string FailingText => string.Join('\n', Failing.Select(result => result.Description));
}
