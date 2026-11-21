// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Probe;

/// <summary>
/// The synthetic probe as a check of the watchdog pipeline, so a public endpoint that is down becomes a finding with a cooldown and an
/// escalation and reaches every channel. An endpoint that fails the first round and answers the second is flaky and not a finding.
/// </summary>
/// <param name="evaluator">The probe evaluator.</param>
/// <param name="targets">The targets.</param>
/// <param name="roundGap">The pause before the second round.</param>
public sealed class ProbeCheck(ProbeEvaluator evaluator, IReadOnlyList<ProbeTarget> targets, TimeSpan roundGap) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "probe";

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        var report = await evaluator.Evaluate(targets, roundGap, cancellationToken);
        return [.. report.Failing.Select(result => new Finding($"probe:{result.Target}/{result.Check}", $"public endpoint {result.Description}"))];
    }
}
