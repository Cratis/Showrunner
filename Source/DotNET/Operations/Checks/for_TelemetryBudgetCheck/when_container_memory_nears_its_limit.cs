// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_TelemetryBudgetCheck;

public class when_container_memory_nears_its_limit : a_telemetry_budget_check
{
    int _atNinetyPercent;
    string _telemetryMessage;
    string _otherMessage;
    string _key;

    async Task Because()
    {
        _prometheus.Vectors[MemoryQuery] = [Tempo(.9)];
        await Run();
        _atNinetyPercent = _findings.Count;

        _prometheus.Vectors[MemoryQuery] = [Tempo(.95), FakePrometheusReader.Sample(.98, ("namespace", "infrastructure"), ("pod", "mongo-rs0-0"), ("container", "mongod"))];
        await Run();
        _telemetryMessage = _findings.Single().Text;
        _key = _findings.Single().Key;

        _prometheus.Vectors[MemoryQuery] = [FakePrometheusReader.Sample(.95, ("namespace", "infrastructure"), ("pod", "decision-1"), ("container", "decision-engine"))];
        _prometheus.Vectors[LimitsQuery] = [FakePrometheusReader.Sample(1073741824, ("namespace", "infrastructure"), ("pod", "decision-1"), ("container", "decision-engine"))];
        await Run();
        _otherMessage = _findings.Single().Text;
    }

    [Fact] void should_not_report_exactly_ninety_percent() => _atNinetyPercent.ShouldEqual(0);
    [Fact] void should_keep_the_container_identity_in_the_key() => _key.ShouldContain("tempo-1");
    [Fact] void should_name_the_bytes_and_the_percentage() => _telemetryMessage.ShouldContain("972.8MiB of 1024.0MiB limit (95.0%)");
    [Fact] void should_keep_ingest_and_queue_advice_for_telemetry_containers() => _telemetryMessage.ShouldContain("ingest rate, queues");
    [Fact] void should_exclude_the_cache_dominated_mongod() => _findings.Count.ShouldEqual(1);
    [Fact] void should_name_the_workload_of_another_container() => _otherMessage.ShouldContain("infrastructure/decision-1/decision-engine");
    [Fact] void should_give_neutral_advice_for_other_containers() => _otherMessage.ShouldNotContain("ingest");
    [Fact] void should_not_mention_queues_for_other_containers() => _otherMessage.ShouldNotContain("queue");
    [Fact] void should_say_there_is_an_oom_risk() => _otherMessage.ShouldContain("OOM risk");
}
#endif
