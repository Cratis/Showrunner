// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Gating;

/// <summary>
/// Decides from the product's declared modules and the tenant's <see cref="IEntitlements"/> whether a namespace is
/// admitted. A trial or a grant that has ended no longer admits, from the day after its last day and without an event.
/// </summary>
/// <remarks>
/// A product that declared no offering has no module that owns a namespace, so everything is admitted. A module that
/// requires another is admitted on its own entitlement: the operator decides what a tenant gets, and a module's
/// prerequisites are checked when the offering is declared, not when a command runs.
/// </remarks>
/// <param name="entitlements">The <see cref="IEntitlements"/> of the current tenant.</param>
/// <param name="options">The <see cref="BoxOfficeOptions"/>.</param>
/// <param name="logger">The logger.</param>
/// <param name="offering">The product's offering, when it declared one.</param>
public class ModuleGate(IEntitlements entitlements, IOptions<BoxOfficeOptions> options, ILogger<ModuleGate> logger, ProductOffering? offering = null) : IModuleGate
{
    /// <inheritdoc/>
    public async Task<ModuleId?> RefusedModuleOf(string? @namespace)
    {
        if (!options.Value.EnforceModules || offering is null || @namespace is null)
        {
            return null;
        }

        if (offering.ModuleOwning(@namespace) is not { } module || module.Availability == ModuleAvailability.Included)
        {
            return null;
        }

        if (await entitlements.Has(module.Id))
        {
            return null;
        }

        logger.ModuleNotEntitled(module.Id, @namespace);
        return module.Id;
    }

    /// <inheritdoc/>
    public async Task<bool> Admits(object artifact) =>
        await RefusedModuleOf((artifact as Type ?? artifact.GetType()).Namespace) is null;
}
