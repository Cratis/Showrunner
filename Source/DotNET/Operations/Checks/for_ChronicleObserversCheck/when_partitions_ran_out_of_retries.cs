// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ChronicleObserversCheck;

public class when_partitions_ran_out_of_retries : a_chronicle_observers_check
{
    const string Key = "chronicle:studio/acme/sender:PartitionsQuarantined";
    IReadOnlyCollection<Alerts.Finding> _first;
    IReadOnlyCollection<Alerts.Finding> _second;

    DateTimeOffset _begin;

    void Establish()
    {
        _begin = _time.Now;
        _chronicle.Stores["studio"]["acme"] = [Partition("sender", 12 * 24 * 60, quarantined: true), Partition("sender", 60, quarantined: true)];
    }

    async Task Because()
    {
        await Run();
        _first = _findings;
        NextRun();
        _time.Advance(TimeSpan.FromHours(7));
        await Run();
        _second = _findings;
    }

    [Fact] void should_report_the_observer() => _first.Select(finding => finding.Key).ShouldContainOnly(Key);
    [Fact] void should_keep_reporting_it_for_as_long_as_it_stays_quarantined() => _second.Select(finding => finding.Key).ShouldContainOnly(Key);
    [Fact] void should_count_the_partitions() => _first.Single().Text.ShouldContain("2 partition(s) ran out of retries");
    [Fact] void should_say_when_the_oldest_failure_was() => _first.Single().Text.ShouldContain("failing since " + _begin.AddDays(-12).UtcDateTime.ToString("yyyy-MM-dd HH:mm") + " UTC");
    [Fact] void should_say_how_to_inspect_it() => _first.Single().Text.ShouldContain("-e studio -n acme");
}
#endif
