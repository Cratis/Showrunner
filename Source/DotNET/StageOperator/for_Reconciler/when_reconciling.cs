// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Kubernetes;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.StageOperator.for_Reconciler;

public class when_reconciling : Specification
{
    IKubernetesGateway _gateway = null!;
    TimeProvider _time = null!;
    List<string> _calls = null!;
    StageApplicationStatus? _written;
    TimeSpan? _requeue;

    void Establish()
    {
        _calls = [];
        _gateway = Substitute.For<IKubernetesGateway>();
        _time = Substitute.For<TimeProvider>();
        _time.GetUtcNow().Returns(Applications.Now);

        var application = Applications.Create(withFinalizer: false);
        _gateway.GetApplication("published", "orders", Arg.Any<CancellationToken>()).Returns(application);
        _gateway.Observe(application, Arg.Any<CancellationToken>()).Returns(new ObservedState(null, false));
        _gateway.AddFinalizer(Arg.Any<StageApplication>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask).AndDoes(_ => _calls.Add("finalizer"));
        _gateway.Apply(Arg.Any<DesiredObjects>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask).AndDoes(_ => _calls.Add("apply"));
        _gateway.UpdateStatus(Arg.Any<StageApplication>(), Arg.Any<Func<StageApplicationStatus, StageApplicationStatus>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(call =>
            {
                _calls.Add("status");
                var latest = new StageApplicationStatus { ResourceUsage = new StageApplicationUsage { Measured = true, SampleCount = 7 } };
                _written = call.Arg<Func<StageApplicationStatus, StageApplicationStatus>>()(latest);
            });
    }

    async Task Because()
    {
        var reconciler = new Reconciler(_gateway, Options.Create(new StageOperatorOptions()), _time, NullLogger<Reconciler>.Instance);
        _requeue = await reconciler.Reconcile("published", "orders", CancellationToken.None);
    }

    [Fact] void should_add_the_finalizer_before_creating_anything() => Assert.Equal(["finalizer", "apply", "status"], _calls);
    [Fact] void should_apply_the_planned_objects() => _gateway.Received(1).Apply(Arg.Is<DesiredObjects>(d => d.Deployment.Metadata.Name == "orders"), Arg.Any<CancellationToken>());
    [Fact] void should_write_the_planned_status() => Assert.Equal(StageApplicationPhases.Pending, _written!.Phase);
    [Fact] void should_keep_the_usage_the_sampler_wrote_meanwhile() => Assert.Equal(7, _written!.ResourceUsage!.SampleCount);
    [Fact] void should_ask_to_be_called_again_while_rolling_out() => Assert.Equal(TimeSpan.FromSeconds(5), _requeue);
    [Fact] void should_not_stop_or_delete_anything() => _gateway.DidNotReceiveWithAnyArgs().DeleteData(default!, default);
}
#endif
