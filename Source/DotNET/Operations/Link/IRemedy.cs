// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// What a remedy does once the link has decided it may run. Direct decides <em>whether</em> - policy, authorization, budget,
/// cooldown, evidence - and the link re-checks it; a remedy only acts, and must be safe to run twice.
/// </summary>
public interface IRemedy
{
    /// <summary>
    /// Gets the name of the remedy in Direct that this implementation executes.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the remedy.
    /// </summary>
    /// <param name="invocation">What is being remedied.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How it went.</returns>
    Task<RemedyResult> Execute(RemedyInvocation invocation, CancellationToken cancellationToken);
}
