// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Catalog.given;

/// <summary>
/// A command scenario for a command on an offer.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public class an_offer_command<TCommand> : Specification
    where TCommand : class
{
    protected CommandScenario<TCommand> _scenario;
    protected OfferId _offerId;
    protected CommandResult _result;

    /// <summary>
    /// Gets the principal the command runs as: the operator of the product, signed in.
    /// </summary>
    protected virtual ClaimsPrincipal Operator => new(new ClaimsIdentity([new Claim("sub", "operator")], "Specifications"));

    void Establish()
    {
        _offerId = OfferId.New();
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Operator));
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds events on the offer before the command runs.
    /// </summary>
    /// <param name="events">The events, in the order they happened; arrays of events are flattened.</param>
    protected void Given(params object[] events) => _scenario.Given.ForEventSource(_offerId).Events(OfferEvents.Then(events));
}

/// <summary>
/// An <see cref="ICurrentPrincipalAccessor"/> that gives a fixed principal.
/// </summary>
/// <param name="principal">The principal.</param>
public class FixedPrincipal(ClaimsPrincipal principal) : ICurrentPrincipalAccessor
{
    /// <inheritdoc/>
    public ClaimsPrincipal? Current => principal;
}
#endif
