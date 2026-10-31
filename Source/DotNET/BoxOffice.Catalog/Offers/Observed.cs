// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// Shapes what the offer queries observe.
/// </summary>
static class Observed
{
    /// <summary>
    /// Orders every emission of an observed list of offers by name.
    /// </summary>
    /// <param name="offers">The observed offers.</param>
    /// <returns>The same offers, ordered by name each time.</returns>
    internal static ISubject<IEnumerable<Offer>> OrderedByName(ISubject<IEnumerable<Offer>> offers) =>
        Subject.Create<IEnumerable<Offer>>(
            Observer.Create<IEnumerable<Offer>>(_ => { }, offers.OnError, offers.OnCompleted),
            offers.Select(list => (IEnumerable<Offer>)[.. list.OrderBy(offer => (string)offer.Name, StringComparer.Ordinal)]));
}
