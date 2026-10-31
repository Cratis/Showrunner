// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleGate.given;

/// <summary>
/// A gate over a product that sells analytics and reports, includes a core, and a tenant whose entitlements the
/// specification gives.
/// </summary>
public class a_module_gate : some_entitlements
{
    /// <summary>
    /// The namespace of analytics, a paid module.
    /// </summary>
    protected const string Analytics = "Acme.Analytics";

    /// <summary>
    /// The namespace of the core, which is included in the product.
    /// </summary>
    protected const string Core = "Acme.Core";

    /// <summary>
    /// The namespace of reports, a module that is only open as a preview.
    /// </summary>
    protected const string Reports = "Acme.Reports";

    protected BoxOfficeOptions _options;
    protected ModuleGate _gate;
    protected ProductOffering _offering;

    void Establish()
    {
        _options = new();
        _offering = new ProductOfferingBuilder()
            .Product(ProductId.New())
            .Module("core", "Core", module => module.Availability(ModuleAvailability.Included).Owns(Core))
            .Module("analytics", "Analytics", module => module.Owns(Analytics))
            .Module("reports", "Reports", module => module.Availability(ModuleAvailability.Preview).Owns(Reports))
            .Build();
        _gate = CreateGate(_offering);
    }

    /// <summary>
    /// Creates a gate over an offering.
    /// </summary>
    /// <param name="offering">The offering, or <see langword="null"/> when the product declared none.</param>
    /// <returns>The gate.</returns>
    protected ModuleGate CreateGate(ProductOffering? offering) =>
        new(_entitlements, Options.Create(_options), Microsoft.Extensions.Logging.Abstractions.NullLogger<ModuleGate>.Instance, offering);
}
#endif
