// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_PodsCheck;

public class when_pods_cannot_be_read : a_check
{
    protected override IWatchdogCheck Check => new PodsCheck(_reader);

    void Establish() => _reader.Failing[nameof(IClusterReader.ListPods)] = 403;

    async Task Because() => await Run();

    [Fact] void should_not_treat_an_unreadable_source_as_healthy() => Keys.ShouldContainOnly("pods:api");
    [Fact] void should_tell_the_reader_where_to_look() => _findings.Single().Text.ShouldContain("inspect watchdog API access");
}
#endif
