// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Chronicle.Testing.Queries;
using Cratis.Arc.Identity;
using Cratis.Arc.Queries;
using Cratis.Arc.Testing.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Catalog.given;

/// <summary>
/// A query scenario for the offer read model, run as a signed-in operator unless the specification says otherwise.
/// </summary>
public class a_catalog_query : Specification
{
    protected QueryScenario<Offer> _scenario;
    protected QueryResult _result;

    /// <summary>
    /// Gets the principal the query runs as; a signed-in operator by default.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => new(new ClaimsIdentity([new Claim("sub", "operator")], "Specifications"));

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds events on an offer.
    /// </summary>
    /// <param name="offerId">The offer.</param>
    /// <param name="events">The events, in the order they happened; arrays of events are flattened.</param>
    protected void Given(OfferId offerId, params object[] events) => _scenario.Given.ForEventSource(offerId).Events(OfferEvents.Then(events));
}
#endif
