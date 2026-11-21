// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ActionsScheduleAuditCheck;

public class when_a_schedule_stops_firing : an_actions_audit
{
    IReadOnlyCollection<Alerts.Finding> _stalled;
    IReadOnlyCollection<Alerts.Finding> _atTheThreshold;
    IReadOnlyCollection<Alerts.Finding> _recent;
    IReadOnlyCollection<Alerts.Finding> _fastScheduleWithinTheFloor;
    IReadOnlyCollection<Alerts.Finding> _fastScheduleBeyondTheFloor;

    async Task Because()
    {
        // The runs are an hour apart, so the threshold is two hours.
        HourlyWorkflow("Cratis/app", Hours(5));
        await Run();
        _stalled = _findings;

        _audit = null;
        HourlyWorkflow("Cratis/app", Hours(2));
        await Run();
        _atTheThreshold = _findings;

        _audit = null;
        HourlyWorkflow("Cratis/app", Hours(1.5));
        await Run();
        _recent = _findings;

        // Every five minutes would be a ten minute threshold, but one hour is the floor so a fast schedule does not flap.
        _audit = null;
        Workflows("Cratis/app", Active(1, "fast.yml"));
        ScheduledRuns("Cratis/app", (1, TimeSpan.FromMinutes(30)), (1, TimeSpan.FromMinutes(35)), (1, TimeSpan.FromMinutes(40)));
        await Run();
        _fastScheduleWithinTheFloor = _findings;

        _audit = null;
        ScheduledRuns("Cratis/app", (1, TimeSpan.FromMinutes(61)), (1, TimeSpan.FromMinutes(66)), (1, TimeSpan.FromMinutes(71)));
        await Run();
        _fastScheduleBeyondTheFloor = _findings;
    }

    [Fact] void should_report_the_age_and_the_inferred_interval() =>
        _stalled.Single().Text.ShouldEqual("GitHub workflow Cratis/app/.github/workflows/nightly.yml: last scheduled run 18000s ago; inferred interval 3600s");
    [Fact] void should_name_the_workflow_in_the_key() => _stalled.Single().Key.ShouldEqual("actions-audit:stalled:Cratis/app:.github/workflows/nightly.yml");
    [Fact] void should_treat_exactly_twice_the_interval_as_healthy() => _atTheThreshold.ShouldBeEmpty();
    [Fact] void should_treat_a_recent_run_as_healthy() => _recent.ShouldBeEmpty();
    [Fact] void should_not_flap_inside_the_one_hour_floor() => _fastScheduleWithinTheFloor.ShouldBeEmpty();
    [Fact] void should_report_beyond_the_one_hour_floor() => _fastScheduleBeyondTheFloor.Single().Key.ShouldEqual("actions-audit:stalled:Cratis/app:fast.yml");
}
#endif
