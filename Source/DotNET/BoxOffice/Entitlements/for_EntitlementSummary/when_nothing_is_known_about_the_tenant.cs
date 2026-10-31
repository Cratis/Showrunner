// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Entitlements.for_EntitlementSummary;

public class when_nothing_is_known_about_the_tenant : a_summary_over_entitlements
{
    EntitlementSummary _summary;

    async Task Because() => _summary = await Summarize();

    [Fact] void should_not_be_a_trial() => _summary.IsTrial.ShouldBeFalse();
    [Fact] void should_have_no_trial_end() => _summary.TrialEnds.ShouldBeNull();
    [Fact] void should_have_no_seats() => _summary.Seats.ShouldEqual(0);
    [Fact] void should_have_no_end_of_the_seats() => _summary.SeatsUntil.ShouldBeNull();
    [Fact] void should_list_every_declared_module() => _summary.Modules.Count().ShouldEqual(4);
    [Fact] void should_give_no_access_to_a_module_that_is_not_included() => _summary.Modules.Where(_ => _.Module != "core").All(_ => _.Access == ModuleAccess.None).ShouldBeTrue();
    [Fact] void should_list_the_modules_in_the_order_of_their_names() => _summary.Modules.Select(_ => _.Name.Value).ShouldEqual(["Agents", "Analytics", "Core", "Reports"]);
}
#endif
