// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_the_production_does_not_report_a_connection_secret : a_preparation
{
    void Establish()
    {
        _performance.Spec.Chronicle = null;
        _performance.Spec.ProductionRef = new() { Name = "ledger" };
    }

    Task Because() => Prepare();

    [Fact] void should_not_prepare_the_performance() => _prepared.ShouldBeNull();
    [Fact] void should_be_blocked() => _status.Phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_skip_the_sinks_and_the_environment() => Conditions.ShouldEqual("SpecValid=True/Valid | ChronicleConnection=False/ChronicleNotAvailable");
    [Fact] void should_say_the_production_has_no_connection_secret() => Condition("ChronicleConnection").Message.ShouldEqual("The Production 'ledger' does not exist or reports no connection Secret yet.");
    [Fact] void should_not_resolve_the_environment() => _resolver.Called.ShouldBeFalse();
}
#endif
