// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.Tenancy;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.GuestList.Authoring.given;

/// <summary>
/// A command scenario for a command on the guest list of a product, run as an operator the host's authoring policy admits
/// unless the specification says otherwise, against a product that has the tenants <see cref="Guests.Acme"/> and
/// <see cref="Guests.Globex"/>.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public class a_guest_list_command<TCommand> : Specification
    where TCommand : class
{
    protected CommandScenario<TCommand> _scenario;
    protected CommandResult _result;

    /// <summary>
    /// Gets the principal the command runs as: an operator, signed in.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => Operators.Operator;

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Principal));
        _scenario.Services.AddArcAuthorizationPolicy<AdmitOperators>(GuestListAuthoring.Policy);
        _scenario.Services.AddSingleton<IKnownTenants>(new TenantsOfTheProduct());
    }

    void Destroy() => _scenario.Dispose();

    /// <summary>
    /// Seeds decisions on the product before the command runs.
    /// </summary>
    /// <param name="events">The events, in the order they happened.</param>
    protected void Given(params object[] events) => _scenario.Given.ForEventSource(new EventSourceId(Guests.Product.Value)).Events(events);
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

/// <summary>
/// The tenants of the specifications' product: <see cref="Guests.Acme"/> and <see cref="Guests.Globex"/>.
/// </summary>
public class TenantsOfTheProduct : IKnownTenants
{
    /// <inheritdoc/>
    public Task<bool> Exists(Product product, TenantId tenant, CancellationToken cancellationToken = default) =>
        Task.FromResult(product == Guests.Product && (tenant == Guests.Acme || tenant == Guests.Globex));
}
#endif
