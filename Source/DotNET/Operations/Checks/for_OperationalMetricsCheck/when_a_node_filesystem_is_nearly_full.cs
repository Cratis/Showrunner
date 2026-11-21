// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_OperationalMetricsCheck;

public class when_a_node_filesystem_is_nearly_full : an_operational_metrics_check
{
    void Establish() => _prometheus.Vectors[RootUsage] = [FakePrometheusReader.Sample(.851, ("node", "node-a"))];

    async Task Because() => await Run();

    [Fact] void should_report_the_node() => Keys.ShouldContain("node:node-a:root-usage");
    [Fact] void should_name_the_subject_and_the_usage() => _findings.Single().Text.ShouldContain("node node-a root filesystem 85.1% used");
}
#endif
