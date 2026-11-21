// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_ReadinessCheck;

public class when_the_pods_cannot_be_listed : a_check
{
    StubHttpHandler _handler;

    protected override IWatchdogCheck Check => new ReadinessCheck(_reader, new HttpClient(_handler), new ReadinessCheckOptions { Namespace = "core", LabelSelector = "app=core" }, _time);

    void Establish()
    {
        _handler = StubHttpHandler.Answering(HttpStatusCode.OK);
        _reader.Failing[nameof(IClusterReader.ListPods)] = 403;
    }

    async Task Because() => await Run();

    [Fact] void should_report_that_readiness_could_not_be_checked() => Keys.ShouldContainOnly("readiness:core:list-pods");
    [Fact] void should_not_probe_anything() => _handler.Requests.ShouldBeEmpty();
}
#endif
