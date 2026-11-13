// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability.for_ProductAvailability.when_reducing;

/// <summary>
/// The same product reports from Staging and from Production: each environment keeps its own last declaration, and
/// the audiences that count are the primary environment's.
/// </summary>
public class two_environments : a_product_that_reports<ProductAvailability>
{
    async Task Because() => await Happened(
        Declares(Production, "1.0.0", Guests.Plus),
        Declares(Staging, "1.1.0", Guests.Plus, Guests.Preview));

    [Fact] void should_hold_each_environment() => Built.Environments.Select(_ => _.Environment.Value).ShouldEqual(["Production", "Staging"]);
    [Fact] void should_keep_what_production_declares() => Built.Environments.First().Audiences.Select(_ => _.Name.Value).ShouldEqual(["Plus"]);
    [Fact] void should_count_the_primary_environment() => Built.DeclaredAudiences(Production)!.Select(_ => _.Value).ShouldEqual(["Plus"]);
    [Fact] void should_know_nothing_of_an_environment_that_did_not_report() => Built.DeclaredAudiences("Canary").ShouldBeNull();
    [Fact] void should_add_everything_of_a_new_environment() => Built.Changes.Last().Added.Select(_ => _.Name).ShouldEqual(["Plus", "Preview"]);
    [Fact] void should_say_which_environment_changed() => Built.Changes.Last().Environment.ShouldEqual(Staging);
}
#endif
