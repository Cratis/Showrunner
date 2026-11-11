// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_preparing_a_published_performance_with_notes : a_preparation
{
    protected override Performance CreatePerformance() => PerformanceSamples.PublishedPerformance();

    Task Because() => Prepare();

    [Fact] void should_prepare_the_performance() => _prepared.ShouldNotBeNull();
    [Fact] void should_use_the_notes_token() => _prepared!.NotesToken.ShouldEqual(new ResolvedSecretRef("crm-notes-token", PublishedStageContract.NotesTokenFile));
    [Fact] void should_record_the_notes_token_after_the_storage() => Conditions.ShouldEqual("SpecValid=True/Valid | PackageAvailable=True/PackageAvailable | StorageConnection=True/Resolved | NotesCollectorToken=True/Resolved | EnvironmentResolved=True/Resolved | EnvironmentComplete=True/Complete");
}
#endif
