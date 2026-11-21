// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ChronicleObserversCheck;

public class when_partitions_are_still_being_retried : a_chronicle_observers_check
{
    IReadOnlyCollection<Alerts.Finding> _recent;
    IReadOnlyCollection<Alerts.Finding> _old;
    IReadOnlyCollection<Alerts.Finding> _resolved;

    async Task Because()
    {
        _chronicle.Stores["studio"]["acme"] = [Partition("projector", 3)];
        await Run();
        _recent = _findings;
        NextRun();

        _chronicle.Stores["studio"]["acme"] = [Partition("projector", 20)];
        await Run();
        _old = _findings;
        NextRun();

        _chronicle.Stores["studio"]["acme"] = [Partition("sender", 600, quarantined: true, resolved: true)];
        await Run();
        _resolved = _findings;
    }

    [Fact] void should_not_report_a_partition_within_the_grace_period() => _recent.ShouldBeEmpty();
    [Fact] void should_report_it_once_the_grace_period_has_passed() => _old.Select(finding => finding.Key).ShouldContainOnly("chronicle:studio/acme/projector:PartitionsFailing");
    [Fact] void should_say_it_is_still_being_retried() => _old.Single().Text.ShouldContain("1 partition(s) still being retried");
    [Fact] void should_not_report_a_resolved_partition() => _resolved.ShouldBeEmpty();
}
#endif
