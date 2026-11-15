// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.Catalog;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupLayout;

public class when_building_the_layout : Specification
{
    static readonly DateTimeOffset _takenAt = new(2026, 3, 15, 14, 5, 9, TimeSpan.FromHours(2));

    [Fact] void should_lay_a_namespace_run_out_by_production_store_namespace_and_time() => BackupLayout.RunPrefix("ledger", "Studio", "acme", _takenAt).ShouldEqual("ledger/Studio/acme/20260315T120509Z/");
    [Fact] void should_use_a_placeholder_for_the_namespace_of_an_event_store_run() => BackupLayout.RunPrefix("ledger", "Studio", null, _takenAt).ShouldEqual("ledger/Studio/_/20260315T120509Z/");
    [Fact] void should_use_placeholders_for_a_production_run() => BackupLayout.RunPrefix("ledger", null, null, _takenAt).ShouldEqual("ledger/_/_/20260315T120509Z/");
    [Fact] void should_put_the_manifest_beside_the_files() => BackupLayout.ManifestKey("ledger/Studio/acme/20260315T120509Z/").ShouldEqual("ledger/Studio/acme/20260315T120509Z/manifest.json");
    [Fact] void should_find_the_run_a_file_belongs_to() => BackupLayout.RunPrefixOf("ledger/Studio/acme/20260315T120509Z/Events.archive.gz").ShouldEqual("ledger/Studio/acme/20260315T120509Z/");
    [Fact] void should_not_take_the_plan_report_for_a_run() => BackupLayout.RunPrefixOf("ledger/plan.json").ShouldBeNull();
    [Fact] void should_sort_by_time() => string.CompareOrdinal(BackupLayout.FormatTakenAt(_takenAt), BackupLayout.FormatTakenAt(_takenAt.AddSeconds(1))).ShouldBeLessThan(0);
}
#endif
