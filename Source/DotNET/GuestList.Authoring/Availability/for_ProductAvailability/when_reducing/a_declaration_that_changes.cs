// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Authoring.given;

namespace Cratis.Showrunner.GuestList.Authoring.Availability.for_ProductAvailability.when_reducing;

/// <summary>
/// A product reports Plus and Preview, then drops Preview, adds Internal and keeps Plus: the declaration is the last one,
/// and the change says what was added and what was removed.
/// </summary>
public class a_declaration_that_changes : a_product_that_reports<ProductAvailability>
{
    async Task Because() => await Happened(
        Declares(Production, "1.0.0", Guests.Plus, Guests.Preview),
        Declares(Production, "1.1.0", Guests.Plus, "Internal"));

    [Fact] void should_belong_to_the_product() => Built.Product.ShouldEqual(Guests.Product);
    [Fact] void should_hold_one_environment() => Built.Environments.Count().ShouldEqual(1);
    [Fact] void should_hold_the_last_version() => Built.Environments.Single().Version.Value.ShouldEqual("1.1.0");
    [Fact] void should_say_when_it_was_declared() => Built.Environments.Single().DeclaredAt.ShouldEqual(At(1));
    [Fact] void should_hold_the_audiences_of_the_last_declaration() => Built.Environments.Single().Audiences.Select(_ => _.Name.Value).ShouldEqual(["Internal", "Plus"]);
    [Fact] void should_have_a_change_for_each_report() => Built.Changes.Count().ShouldEqual(2);
    [Fact] void should_add_everything_in_the_first_report() => Built.Changes.First().Added.Select(_ => _.Name).ShouldEqual(["Plus", "Preview"]);
    [Fact] void should_remove_nothing_in_the_first_report() => Built.Changes.First().Removed.ShouldBeEmpty();
    [Fact] void should_add_what_is_new() => Built.Changes.Last().Added.Select(_ => _.Name).ShouldEqual(["Internal"]);
    [Fact] void should_remove_what_is_gone() => Built.Changes.Last().Removed.Select(_ => _.Name).ShouldEqual(["Preview"]);
    [Fact] void should_not_call_what_stayed_changed() => Built.Changes.Last().Changed.ShouldBeEmpty();
    [Fact] void should_say_which_version_changed_it() => Built.Changes.Last().Version.Value.ShouldEqual("1.1.0");
}
#endif
