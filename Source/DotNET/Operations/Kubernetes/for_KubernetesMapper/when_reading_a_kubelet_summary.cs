// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;

namespace Cratis.Showrunner.Operations.Kubernetes.for_KubernetesMapper;

public class when_reading_a_kubelet_summary : Specification
{
    const string Summary = """{"node":{"nodeName":"n1"},"pods":[{"podRef":{"name":"db-0","namespace":"shop"},"volume":[{"name":"data","capacityBytes":107374182400,"usedBytes":53687091200,"pvcRef":{"name":"data-db-0","namespace":"shop"}},{"name":"config","capacityBytes":1000,"usedBytes":1},{"name":"no-usage","pvcRef":{"name":"x","namespace":"shop"}}]},{"podRef":{"name":"web-0","namespace":"shop"}}]}""";

    IReadOnlyList<VolumeUsage> _usage;
    Exception _error;

    void Because()
    {
        _usage = KubernetesMapper.VolumeUsage(Summary);
        static void Read() => KubernetesMapper.VolumeUsage("""{"node":{}}""");
        _error = Catch.Exception(Read);
    }

    [Fact] void should_read_the_usage_of_claims() => _usage.Single().ShouldEqual(new VolumeUsage("shop", "data-db-0", 107374182400, 53687091200));
    [Fact] void should_skip_volumes_that_are_not_claims() => _usage.Count.ShouldEqual(1);
    [Fact] void should_reject_a_summary_without_pod_usage_so_the_node_is_reported() => _error.ShouldBeOfExactType<ClusterReadException>();
}
#endif
