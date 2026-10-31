// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOffering.when_finding_the_module_owning_a_namespace;

public class a_namespace_that_only_shares_a_prefix : given_modules
{
    ModuleDefinition _module;

    void Because() => _module = _offering.ModuleOwning("Acme.AnalyticsExtras");

    [Fact] void should_belong_to_the_module_owning_the_parent() => _module.Id.Value.ShouldEqual("core");
}
#endif
