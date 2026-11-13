// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Choreography.Availability.Context;
using Cratis.Choreography.Availability.Context.Providers;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;

namespace Cratis.Showrunner.BoxOffice.Product.Availability;

/// <summary>
/// Supplies what Box Office entitles the current tenant to as the host-provided context value <c>entitlements</c>, so a
/// model that says <c>provide entitlements EntitlementsValue from host</c> can read it in an <c>available when</c> condition.
/// </summary>
/// <remarks>
/// Nothing is read until a gate reads <c>$context.entitlements</c>, and the engine reads it once per request. Box Office is
/// not changed and does not know the engine: this only reads <see cref="IEntitlements"/>, which answers for the tenant of the
/// request. Any other name is not supplied, so a host that supplies more values than this one delegates the others itself.
/// </remarks>
/// <param name="entitlements">Gets the <see cref="IEntitlements"/> of the request being answered.</param>
/// <param name="offering">The product's offering, which names the modules; without one no module is listed.</param>
public class EntitlementsHostContextValues(Func<IEntitlements> entitlements, ProductOffering? offering = null) : IHostContextValues
{
    /// <summary>
    /// The name the model declares the value by.
    /// </summary>
    public const string Name = "entitlements";

    /// <inheritdoc/>
    public async ValueTask<ContextValueOutcome> Get(string name, CancellationToken cancellationToken)
    {
        if (!string.Equals(name, Name, StringComparison.OrdinalIgnoreCase))
        {
            return ContextValueOutcome.Absent;
        }

        var current = entitlements();
        var reason = await current.WhyReadOnly();
        var seats = await current.SeatCapacity();

        var modules = new Dictionary<string, ModuleEntitlementValue>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in offering?.Modules ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            var access = module.Availability == ModuleAvailability.Included ? ModuleAccess.Full : await current.AccessTo(module.Id);
            modules[module.Id.Value] = new(access.ToString(), access is ModuleAccess.Trial or ModuleAccess.Full);
        }

        return ContextValueOutcome.Present(new EntitlementsValue(reason == ReadOnlyReason.None, reason.ToString(), seats, modules));
    }
}
