// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.given;

/// <summary>
/// A command run through Arc's pipeline in a tenant, with Box Office added.
/// </summary>
/// <typeparam name="TCommand">The command.</typeparam>
public class a_seat_command<TCommand> : Specification
    where TCommand : class
{
    protected CommandScenario<TCommand> _scenario;
    protected CommandResult _result;

    /// <summary>
    /// Gets the principal the command runs as; a maintainer of the product by default.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => Tenants.SignedInAs(Tenants.Ada);

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddArcAuthorizationPolicy<TheProductsMaintainers>(BoxOfficePolicies.ManageSeats);
        _scenario.Services.AddArcAuthorizationPolicy<TheProductsMaintainers>(BoxOfficePolicies.ManageCompute);
        _scenario.Services.AddBoxOffice();
        _scenario.Clock(Tenants.Now);
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// The product's policy for who may manage seats and compute: whoever is signed in.
    /// </summary>
    protected class TheProductsMaintainers : IAuthorizationPolicy
    {
        /// <inheritdoc/>
        public ValueTask<bool> IsAuthorized(AuthorizationPolicyContext context, CancellationToken cancellationToken) =>
            ValueTask.FromResult(context.Principal.Identity?.IsAuthenticated == true);
    }

    /// <summary>
    /// Seeds the facts of the tenant's stream before the command runs.
    /// </summary>
    /// <param name="facts">The facts, in the order they happened.</param>
    protected void Given(params object[] facts) => _scenario.Given.ForEventSource(TenantStream.Id).Events(facts);
}
#endif
