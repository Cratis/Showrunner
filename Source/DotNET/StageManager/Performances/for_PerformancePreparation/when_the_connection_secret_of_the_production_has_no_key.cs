// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_the_connection_secret_of_the_production_has_no_key : a_preparation
{
    void Establish()
    {
        _performance.Spec.Chronicle = null;
        _performance.Spec.ProductionRef = new() { Name = "ledger" };
        _cluster.ProductionConnectionSecrets["apps/ledger"] = "ledger-connection";
        _cluster.Productions["apps/ledger"] = ProductionSamples.WithSinks();
    }

    Task Because() => Prepare();

    [Fact] void should_not_prepare_the_performance() => _prepared.ShouldBeNull();
    [Fact] void should_be_blocked() => _status.Phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_not_check_the_sinks() => Conditions.ShouldEqual("SpecValid=True/Valid | ChronicleConnection=False/ChronicleNotAvailable");
    [Fact] void should_say_the_key_is_missing() => Condition("ChronicleConnection").Message.ShouldEqual($"The connection Secret 'ledger-connection' has no key '{ChronicleConnectionRef.DefaultKey}'.");
}
#endif
