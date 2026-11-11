// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_the_passphrase_secret_is_missing : a_preparation
{
    void Establish() => _performance.Spec.Environment.PassphraseSecretRef = new() { Name = "crm-passphrase" };

    Task Because() => Prepare();

    [Fact] void should_not_prepare_the_performance() => _prepared.ShouldBeNull();
    [Fact] void should_be_blocked() => _status.Phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_record_the_failure() => Conditions.ShouldEqual("SpecValid=True/Valid | ChronicleConnection=True/Resolved | EnvironmentResolved=False/PassphraseNotFound");
    [Fact] void should_say_which_secret() => Condition("EnvironmentResolved").Message.ShouldEqual("The passphrase Secret 'crm-passphrase' does not exist or has no such key.");
    [Fact] void should_not_call_the_resolver() => _resolver.Called.ShouldBeFalse();
}
#endif
