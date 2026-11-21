// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_OperationalMetricsCheck;

public class when_subjects_are_missing : an_operational_metrics_check
{
    void Establish() => _prometheus.Vectors.Clear();

    async Task Because() => await Run();

    [Fact] void should_report_the_node_without_filesystem_data() => Keys.ShouldContain("metrics:root-usage:missing:node-a");
    [Fact] void should_report_each_node_job_without_a_target() => Keys.Count(key => key.StartsWith("metrics:node-targets:missing")).ShouldEqual(2);
    [Fact] void should_report_missing_kube_state_metrics() => Keys.ShouldContain("metrics:service-targets:missing:kube-state-metrics");
    [Fact] void should_report_the_missing_collector() => Keys.ShouldContain("metrics:collector-targets:missing:otel-collector");
}
#endif
