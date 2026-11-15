// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Reporting;

namespace Cratis.Showrunner.StageManager.Understudies.given;

/// <summary>
/// A sink that records every fact it is given.
/// </summary>
public class RecordingEventSink : IUnderstudyEventSink
{
    public List<UnderstudyFact> Facts { get; } = [];

    public IEnumerable<T> Payloads<T>() => Facts.Select(fact => fact.Payload).OfType<T>();

    public Task PublishAsync(Understudy understudy, UnderstudyFact fact, CancellationToken cancellationToken)
    {
        Facts.Add(fact);
        return Task.CompletedTask;
    }
}
#endif
