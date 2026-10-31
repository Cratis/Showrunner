// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOffering.when_finding_the_module_owning_a_namespace;

public class a_namespace_inside_two_owned_ones : given_modules
{
    ModuleDefinition _module;

    void Because() => _module = _offering.ModuleOwning("Acme.Analytics.Reports");

    [Fact] void should_find_the_module_with_the_longest_match() => _module.Id.Value.ShouldEqual("analytics");
}
#endif
