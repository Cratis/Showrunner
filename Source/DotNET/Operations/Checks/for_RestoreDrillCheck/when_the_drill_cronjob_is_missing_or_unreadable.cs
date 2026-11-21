// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_RestoreDrillCheck;

public class when_the_drill_cronjob_is_missing_or_unreadable : a_check
{
    IReadOnlyCollection<Cratis.Showrunner.Operations.Alerts.Finding> _missing;

    protected override IWatchdogCheck Check => new RestoreDrillCheck(_reader, "drill", "restore-drill", TimeSpan.FromHours(30));

    async Task Because()
    {
        await Run();
        _missing = _findings;
        _reader.Failing[nameof(IClusterReader.GetCronJob)] = 403;
        await Run();
    }

    [Fact] void should_report_a_drill_that_does_not_exist_although_it_is_enabled() => _missing.Single().Key.ShouldEqual("restore-drill:missing");
    [Fact] void should_tell_the_reader_to_deploy() => _missing.Single().Text.ShouldContain("run the deploy");
    [Fact] void should_report_an_unreadable_cronjob_with_its_status() => _findings.Single().Text.ShouldContain("(HTTP 403)");
    [Fact] void should_not_treat_an_unreadable_source_as_healthy() => Keys.ShouldContainOnly("restore-drill:api");
}
#endif
