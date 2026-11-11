// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_the_environment_file_is_missing : a_preparation
{
    void Establish() => _cluster.ConfigMapValues.Clear();

    Task Because() => Prepare();

    [Fact] void should_not_prepare_the_performance() => _prepared.ShouldBeNull();
    [Fact] void should_be_blocked() => _status.Phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_stop_at_the_environment() => Conditions.ShouldEqual("SpecValid=True/Valid | ChronicleConnection=True/Resolved | EnvironmentResolved=False/EnvironmentFileNotFound");
    [Fact] void should_name_the_default_key() => Condition("EnvironmentResolved").Message.ShouldEqual("The ConfigMap 'crm-environment' has no key 'production.yaml'.");
    [Fact] void should_not_call_the_resolver() => _resolver.Called.ShouldBeFalse();
}
#endif
