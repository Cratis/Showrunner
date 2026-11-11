// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_the_explicit_connection_secret_has_no_key : a_preparation
{
    void Establish() => _cluster.SecretValues.Remove("apps/chronicle-connection/connectionString");

    Task Because() => Prepare();

    [Fact] void should_not_prepare_the_performance() => _prepared.ShouldBeNull();
    [Fact] void should_be_blocked() => _status.Phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_stop_after_the_connection() => Conditions.ShouldEqual("SpecValid=True/Valid | ChronicleConnection=False/ChronicleNotAvailable");
    [Fact] void should_say_the_key_is_missing() => Condition("ChronicleConnection").Message.ShouldEqual($"The connection Secret 'chronicle-connection' has no key '{ChronicleConnectionRef.DefaultKey}'.");
    [Fact] void should_not_resolve_the_environment() => _resolver.Called.ShouldBeFalse();
}
#endif
