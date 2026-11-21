// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ChronicleObserversCheck;

public class when_there_are_many_namespaces : a_chronicle_observers_check
{
    void Establish()
    {
        _chronicle.Stores["studio"] = Enumerable.Range(0, 40).ToDictionary(index => $"tenant{index}", _ => new List<ChronicleFailedPartition>());
        _chronicle.Stores["studio"]["tenant39"] = [Partition("late", 60, quarantined: true)];
    }

    async Task Because() => await Run();

    [Fact] void should_read_every_namespace() => Keys.ShouldContainOnly("chronicle:studio/tenant39/late:PartitionsQuarantined");
    [Fact] void should_read_no_more_than_eight_at_a_time() => (_chronicle.MostInFlight <= 8).ShouldBeTrue();
}
#endif
