// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOffering.when_finding_the_module_owning_a_namespace;

public class given_modules : Specification
{
    protected ProductOffering _offering;

    void Establish() => _offering = new ProductOfferingBuilder()
        .Product(ProductId.New())
        .Module("core", "Core", module => module.Owns("Acme"))
        .Module("analytics", "Analytics", module => module.Owns("Acme.Analytics"))
        .Module("exports", "Exports", module => module.Owns("Acme.Analytics.Exports"))
        .Build();
}
#endif
