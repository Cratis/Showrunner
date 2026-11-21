// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_PbmBackupsCheck;

public class when_the_backups_cannot_be_read : a_check
{
    protected override IWatchdogCheck Check => new PbmBackupsCheck(_reader, "databases", TimeSpan.FromHours(3));

    void Establish() => _reader.Failing[nameof(IClusterReader.ListPbmBackups)] = 403;

    async Task Because() => await Run();

    [Fact] void should_not_treat_an_unreadable_source_as_healthy() => Keys.ShouldContainOnly("pbm-backups:api");
    [Fact] void should_name_the_kind_of_error_and_not_its_text() => _findings.Single().Text.ShouldContain("(ClusterReadException)");
}
#endif
