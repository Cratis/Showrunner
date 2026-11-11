// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_preparing_a_performance_with_an_explicit_connection : a_preparation
{
    Task Because() => Prepare();

    [Fact] void should_prepare_the_performance() => _prepared.ShouldNotBeNull();
    [Fact] void should_not_block() => _status.Phase.ShouldEqual(PerformancePhase.Pending);
    [Fact] void should_use_the_connection_secret_with_the_default_key() => _prepared!.Connection!.Key.ShouldEqual(ChronicleConnectionRef.DefaultKey);
    [Fact] void should_name_the_connection_secret() => _prepared!.Connection!.SecretName.ShouldEqual("chronicle-connection");
    [Fact] void should_have_no_published_storage() => _prepared!.Storage.ShouldBeNull();
    [Fact] void should_have_no_notes_token() => _prepared!.NotesToken.ShouldBeNull();
    [Fact] void should_bind_nothing() => _prepared!.ReadSide.ShouldBeSame(ApplicationReadSide.None);
    [Fact] void should_record_the_conditions_in_order() => Conditions.ShouldEqual("SpecValid=True/Valid | ChronicleConnection=True/Resolved | EnvironmentResolved=True/Resolved | EnvironmentComplete=True/Complete");
    [Fact] void should_say_which_secret_is_used() => Condition("ChronicleConnection").Message.ShouldEqual("Using the connection Secret 'chronicle-connection'.");
    [Fact] void should_say_the_environment_is_complete() => Condition("EnvironmentComplete").Message.ShouldEqual("The environment has every key the Performance needs.");
    [Fact] void should_say_which_environment_was_resolved() => Condition("EnvironmentResolved").Message.ShouldEqual("Resolved the environment 'production'.");
    [Fact] void should_hand_the_environment_file_to_the_resolver() => _resolver.File.ShouldEqual("environment-file");
    [Fact] void should_hand_no_passphrase_to_the_resolver() => _resolver.Passphrase.ShouldBeNull();
}
#endif
