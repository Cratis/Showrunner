// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_preparing_a_published_performance_without_notes : a_preparation
{
    protected override Performance CreatePerformance() => PerformanceSamples.PublishedPerformance(notes: false);

    Task Because() => Prepare();

    [Fact] void should_prepare_the_performance() => _prepared.ShouldNotBeNull();
    [Fact] void should_have_no_connection() => _prepared!.Connection.ShouldBeNull();
    [Fact] void should_have_no_read_side() => _prepared!.ReadSide.ShouldBeNull();
    [Fact] void should_have_no_notes_token() => _prepared!.NotesToken.ShouldBeNull();
    [Fact] void should_use_the_storage_secret() => _prepared!.Storage.ShouldEqual(new ResolvedSecretRef("crm-storage", ChronicleConnectionRef.DefaultKey));
    [Fact] void should_record_the_conditions_in_order() => Conditions.ShouldEqual("SpecValid=True/Valid | PackageAvailable=True/PackageAvailable | StorageConnection=True/Resolved | EnvironmentResolved=True/Resolved | EnvironmentComplete=True/Complete");
    [Fact] void should_say_the_package_is_used() => Condition("PackageAvailable").Message.ShouldEqual("Using the package in the ConfigMap 'crm-package-1-4-0'.");
}
#endif
