// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_a_published_performance_has_nothing_available : a_preparation
{
    protected override Performance CreatePerformance() => PerformanceSamples.PublishedPerformance();

    void Establish()
    {
        _cluster.Packages.Clear();
        _cluster.SecretValues.Clear();
    }

    Task Because() => Prepare();

    [Fact] void should_not_prepare_the_performance() => _prepared.ShouldBeNull();
    [Fact] void should_be_blocked() => _status.Phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_evaluate_the_package_the_storage_and_the_notes_token_without_stopping_at_the_first() => Conditions.ShouldEqual("SpecValid=True/Valid | PackageAvailable=False/PackageNotFound | StorageConnection=False/SecretNotFound | NotesCollectorToken=False/SecretNotFound");
    [Fact] void should_not_resolve_the_environment() => _resolver.Called.ShouldBeFalse();
    [Fact] void should_say_the_package_is_missing() => Condition("PackageAvailable").Message.ShouldEqual("The package ConfigMap 'crm-package-1-4-0' does not exist.");
    [Fact] void should_say_the_storage_secret_is_missing() => Condition("StorageConnection").Message.ShouldEqual("The storage connection Secret 'crm-storage' does not exist or has no key 'connectionString'.");
    [Fact] void should_say_the_notes_token_is_missing() => Condition("NotesCollectorToken").Message.ShouldEqual("The Notes collector token Secret 'crm-notes-token' does not exist or has no key 'token'.");
}
#endif
