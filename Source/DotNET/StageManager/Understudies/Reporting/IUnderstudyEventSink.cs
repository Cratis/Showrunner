// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Reporting;

/// <summary>
/// Somewhere the facts of an Understudy are published. Publishing a fact twice must be harmless: the sink recognizes it by <see cref="UnderstudyFact.Id"/>.
/// </summary>
public interface IUnderstudyEventSink
{
    /// <summary>
    /// Publishes a fact.
    /// </summary>
    /// <param name="understudy">The Understudy the fact is about.</param>
    /// <param name="fact">The fact.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the fact is published, or recognized as published already.</returns>
    Task PublishAsync(Understudy understudy, UnderstudyFact fact, CancellationToken cancellationToken);
}
