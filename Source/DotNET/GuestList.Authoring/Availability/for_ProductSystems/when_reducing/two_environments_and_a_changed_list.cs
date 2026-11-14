// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability.for_ProductSystems.when_reducing;

/// <summary>
/// The same product reports its systems from Staging and from Production, and Production reports a changed list: each report
/// replaces the earlier list of its own environment only, and a system that is no longer listed is no longer offered.
/// </summary>
public class two_environments_and_a_changed_list : a_product_that_reports<ProductSystems>
{
    static readonly SystemDeclaration Crm = new(Guests.Crm, SystemKind.Api, "CRM sync");
    static readonly SystemDeclaration Reports = new(Guests.Reports, SystemKind.Mcp, "Reports assistant");

    async Task Because() => await Happened(
        new SystemsDeclared(Guests.Product, Production, [Reports, Crm]),
        new SystemsDeclared(Guests.Product, Staging, [Crm]),
        new SystemsDeclared(Guests.Product, Production, [Reports]));

    [Fact] void should_belong_to_the_product() => Built.Product.ShouldEqual(Guests.Product);
    [Fact] void should_hold_each_environment() => Built.Environments.Select(_ => _.Environment.Value).ShouldEqual(["Production", "Staging"]);
    [Fact] void should_replace_the_list_of_the_environment_that_reported_again() => Built.Environments.First().Systems.ShouldContainOnly(Reports);
    [Fact] void should_keep_the_list_of_the_other_environment() => Built.Environments.Last().Systems.ShouldContainOnly(Crm);
    [Fact] void should_say_which_kind_each_system_is() => Built.Environments.First().Systems.Single().Kind.ShouldEqual(SystemKind.Mcp);
}
#endif
