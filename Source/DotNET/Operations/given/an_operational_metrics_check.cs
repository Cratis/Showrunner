// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Checks;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// The operational metrics check against a one-node cluster with healthy metrics; a specification spoils what it needs to.
/// </summary>
public abstract class an_operational_metrics_check : a_check
{
    protected const string RootUsage = "1 - node_filesystem_avail";
    protected const string NodeTargets = "job=~\"kubernetes-nodes";
    protected const string ServiceTargets = "job=\"kubernetes-service-endpoints\"";
    protected const string CollectorTargets = "job=\"kubernetes-pods\"";

    protected FakePrometheusReader _prometheus;

    protected override IWatchdogCheck Check => new OperationalMetricsCheck(_reader, _prometheus, new OperationalMetricsOptions());

    void Establish()
    {
        _prometheus = new FakePrometheusReader();
        _reader.Nodes.Add(new NodeInfo("node-a", [new NodeCondition("Ready", "True")]));
        _prometheus.Vectors[RootUsage] = [FakePrometheusReader.Sample(.8, ("node", "node-a"))];
        _prometheus.Vectors[NodeTargets] =
        [
            FakePrometheusReader.Sample(1, ("job", "kubernetes-nodes"), ("instance", "node-a")),
            FakePrometheusReader.Sample(1, ("job", "kubernetes-nodes-cadvisor"), ("instance", "node-a"))
        ];
        _prometheus.Vectors[ServiceTargets] = [FakePrometheusReader.Sample(1, ("service", "release-kube-state-metrics"), ("job", "kubernetes-service-endpoints"), ("instance", "release-kube-state-metrics:1"))];
        _prometheus.Vectors[CollectorTargets] = [FakePrometheusReader.Sample(1, ("job", "kubernetes-pods"), ("instance", "collector:8889"))];
    }
}
#endif
