// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Cratis.Showrunner.Usher.Issues;

/// <summary>
/// Shapes the emissions of an observed read-model collection for an observable query.
/// </summary>
internal static class ObservedQueries
{
    /// <summary>
    /// Maps every emission of an observation, keeping it a subject Arc can serve: disposing the last subscription
    /// disposes the observation underneath.
    /// </summary>
    /// <typeparam name="TSource">The type the observation emits.</typeparam>
    /// <typeparam name="TResult">The type the query emits.</typeparam>
    /// <param name="source">The observation.</param>
    /// <param name="map">Maps one emission.</param>
    /// <returns>The mapped subject.</returns>
    public static ISubject<TResult> Map<TSource, TResult>(ISubject<TSource> source, Func<TSource, TResult> map) =>
        Subject.Create<TResult>(Observer.Create<TResult>(_ => { }, source.OnError, source.OnCompleted), source.Select(map));
}
