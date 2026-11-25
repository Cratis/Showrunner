// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability.for_ProductParts.when_reducing;

/// <summary>
/// The same product reports its parts from Staging and from Production, and Production reports a changed list: each report
/// replaces the earlier list of its own environment only, and a part that is no longer listed is no longer offered.
/// </summary>
public class two_environments_and_a_changed_list : a_product_that_reports<ProductParts>
{
    static readonly PartDeclaration Module = new("module", "Fleet", "Fleet");
    static readonly PartDeclaration Feature = new("feature", "Fleet/Reports", "Reports");

    async Task Because() => await Happened(
        new PartsDeclared(Guests.Product, Production, [Feature, Module]),
        new PartsDeclared(Guests.Product, Staging, [Module]),
        new PartsDeclared(Guests.Product, Production, [Module]));

    [Fact] void should_belong_to_the_product() => Built.Product.ShouldEqual(Guests.Product);
    [Fact] void should_hold_each_environment() => Built.Environments.Select(_ => _.Environment.Value).ShouldEqual(["Production", "Staging"]);
    [Fact] void should_replace_the_list_of_the_environment_that_reported_again() => Built.Environments.First().Parts.ShouldContainOnly(Module);
    [Fact] void should_keep_the_list_of_the_other_environment() => Built.Environments.Last().Parts.ShouldContainOnly(Module);
}
#endif
