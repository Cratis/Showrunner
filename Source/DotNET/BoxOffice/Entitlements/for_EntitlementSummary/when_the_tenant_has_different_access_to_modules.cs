// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_EntitlementSummary;

public class when_the_tenant_has_different_access_to_modules : a_summary_over_entitlements
{
    EntitlementSummary _summary;
    EntitlementSummary _later;

    async Task Because()
    {
        Given(
            new OperatorGrantedModule("core", EntitlementEnd.Never),
            new OperatorGrantedModuleTrial("analytics", new(Tenants.Day(2))),
            new OperatorOpenedModulePreview("agents", new(Tenants.Day(9))));
        _summary = await Summarize();

        _clock.MoveTo(Tenants.Day(3));
        _later = await Summarize();
    }

    ModuleStanding Of(EntitlementSummary summary, string module) => summary.Modules.Single(_ => _.Module == module);

    [Fact] void should_give_full_access_to_a_granted_module() => Of(_summary, "core").Access.ShouldEqual(ModuleAccess.Full);
    [Fact] void should_tell_no_end_for_a_module_that_does_not_end() => Of(_summary, "core").Until.ShouldBeNull();
    [Fact] void should_give_trial_access_to_a_trial_module() => Of(_summary, "analytics").Access.ShouldEqual(ModuleAccess.Trial);
    [Fact] void should_tell_the_last_day_of_the_trial() => Of(_summary, "analytics").Until.ShouldEqual(Tenants.Day(2));
    [Fact] void should_give_preview_access_to_a_previewed_module() => Of(_summary, "agents").Access.ShouldEqual(ModuleAccess.Preview);
    [Fact] void should_withdraw_a_trial_that_has_ended_without_any_event() => Of(_later, "analytics").Access.ShouldEqual(ModuleAccess.None);
    [Fact] void should_tell_no_end_for_access_that_has_ended() => Of(_later, "analytics").Until.ShouldBeNull();
    [Fact] void should_keep_what_is_still_running() => Of(_later, "agents").Access.ShouldEqual(ModuleAccess.Preview);
}
#endif
