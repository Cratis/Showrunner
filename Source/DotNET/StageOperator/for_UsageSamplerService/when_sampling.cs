// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Hosting;
using Cratis.Showrunner.StageOperator.Kubernetes;
using Cratis.Showrunner.StageOperator.Metering;
using Cratis.Showrunner.StageOperator.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.StageOperator.for_UsageSamplerService;

public class when_sampling : Specification
{
    IKubernetesGateway _gateway = null!;
    StageApplication _running = null!;
    StageApplication _expired = null!;
    StageApplication _noMetrics = null!;
    StageApplicationStatus? _written;

    void Establish()
    {
        _running = Applications.Create(status: Applications.Running());
        _expired = Applications.Create(status: new StageApplicationStatus { Phase = StageApplicationPhases.Expired });
        _expired.Metadata.Name = "expired";
        _noMetrics = Applications.Create(status: Applications.Running());
        _noMetrics.Metadata.Name = "quiet";

        _gateway = Substitute.For<IKubernetesGateway>();
        _gateway.ListApplications(null, Arg.Any<CancellationToken>()).Returns([_running, _expired, _noMetrics]);
        _gateway.GetContainerUsage(_running, Arg.Any<CancellationToken>()).Returns(new List<ContainerUsage> { new("orders-1", "stage", 0.5, 100_000_000) });
        _gateway.GetContainerUsage(_noMetrics, Arg.Any<CancellationToken>()).Returns((IReadOnlyList<ContainerUsage>?)null);
        _gateway.UpdateStatus(Arg.Any<StageApplication>(), Arg.Any<Func<StageApplicationStatus, StageApplicationStatus>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(call => _written = call.Arg<Func<StageApplicationStatus, StageApplicationStatus>>()(new StageApplicationStatus()));
    }

    async Task Because()
    {
        var time = Substitute.For<TimeProvider>();
        time.GetUtcNow().Returns(Applications.Now);
        await new UsageSamplerService(_gateway, Options.Create(new StageOperatorOptions()), time, NullLogger<UsageSamplerService>.Instance).Sample(CancellationToken.None);
    }

    [Fact] void should_record_the_usage_of_the_running_application() => Assert.Equal(1, _written!.ResourceUsage!.SampleCount);
    [Fact] void should_write_status_only_once() => _gateway.ReceivedWithAnyArgs(1).UpdateStatus(default!, default!, default);
    [Fact] void should_not_read_metrics_of_an_expired_application() => _gateway.DidNotReceive().GetContainerUsage(_expired, Arg.Any<CancellationToken>());
    [Fact] void should_not_invent_usage_when_metrics_are_not_available() => _gateway.DidNotReceive().UpdateStatus(_noMetrics, Arg.Any<Func<StageApplicationStatus, StageApplicationStatus>>(), Arg.Any<CancellationToken>());
}
#endif
