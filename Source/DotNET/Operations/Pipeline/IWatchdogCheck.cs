// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;

namespace Cratis.Showrunner.Operations.Pipeline;

/// <summary>
/// One concern of the watchdog. A check returns what is wrong and nothing when everything is healthy; it throws only when it
/// cannot tell, and the run turns that into a finding of its own rather than aborting.
/// </summary>
public interface IWatchdogCheck
{
    /// <summary>
    /// Gets the name of the check, used in configuration and in the finding when the check itself fails.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Collects the findings of the check.
    /// </summary>
    /// <param name="context">The run the check is part of.</param>
    /// <param name="cancellationToken">Cancelled when the check has used its time budget.</param>
    /// <returns>The findings; empty when healthy.</returns>
    Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken);
}
