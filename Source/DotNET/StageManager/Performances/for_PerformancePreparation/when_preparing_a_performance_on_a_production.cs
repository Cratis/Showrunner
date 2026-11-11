// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_preparing_a_performance_on_a_production : a_preparation
{
    protected override Performance CreatePerformance()
    {
        var performance = PerformanceSamples.Performance();
        performance.Spec.Chronicle = null;
        performance.Spec.ProductionRef = new() { Name = "ledger" };
        return performance;
    }

    void Establish()
    {
        _cluster.ProductionConnectionSecrets["apps/ledger"] = "ledger-connection";
        _cluster.SecretValues["apps/ledger-connection/connectionString"] = PerformanceSamples.ConnectionString;
        _cluster.Productions["apps/ledger"] = ProductionSamples.WithSinks();
    }

    Task Because() => Prepare();

    [Fact] void should_prepare_the_performance() => _prepared.ShouldNotBeNull();
    [Fact] void should_use_the_connection_secret_of_the_production() => _prepared!.Connection.ShouldEqual(new ChronicleConnectionRef("ledger-connection", ChronicleConnectionRef.DefaultKey));
    [Fact] void should_bind_the_read_side() => _prepared!.ReadSide.ShouldNotBeNull();
    [Fact] void should_record_the_connection_then_the_sinks_then_the_environment() => Conditions.Split(" | ").Select(entry => entry.Split('=')[0]).Where(type => new[] { "ChronicleConnection", "SinksMapped", "EnvironmentResolved" }.Contains(type, StringComparer.Ordinal)).Aggregate(string.Empty, (all, type) => $"{all}{type};").ShouldEqual("ChronicleConnection;SinksMapped;EnvironmentResolved;");
}
#endif
