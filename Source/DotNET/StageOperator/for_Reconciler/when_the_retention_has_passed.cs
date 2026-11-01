// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Kubernetes;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.StageOperator.for_Reconciler;

public class when_the_retention_has_passed : Specification
{
    IKubernetesGateway _gateway = null!;
    List<string> _calls = null!;

    void Establish()
    {
        _calls = [];
        var application = Applications.Create(status: Applications.Running(), deletedAt: Applications.Now.AddDays(-8));
        _gateway = Substitute.For<IKubernetesGateway>();
        _gateway.GetApplication("published", "orders", Arg.Any<CancellationToken>()).Returns(application);
        _gateway.Observe(application, Arg.Any<CancellationToken>()).Returns(new ObservedState(null, true));
        _gateway.StopWorkloads(Arg.Any<StageApplication>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask).AndDoes(_ => _calls.Add("stop"));
        _gateway.DeleteIngress(Arg.Any<StageApplication>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask).AndDoes(_ => _calls.Add("ingress"));
        _gateway.DeleteData(Arg.Any<StageApplication>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask).AndDoes(_ => _calls.Add("data"));
        _gateway.UpdateStatus(Arg.Any<StageApplication>(), Arg.Any<Func<StageApplicationStatus, StageApplicationStatus>>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask).AndDoes(_ => _calls.Add("status"));
        _gateway.RemoveFinalizer(Arg.Any<StageApplication>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask).AndDoes(_ => _calls.Add("release"));
    }

    async Task Because()
    {
        var time = Substitute.For<TimeProvider>();
        time.GetUtcNow().Returns(Applications.Now);
        await new Reconciler(_gateway, Options.Create(new StageOperatorOptions()), time, NullLogger<Reconciler>.Instance).Reconcile("published", "orders", CancellationToken.None);
    }

    [Fact] void should_release_the_finalizer_only_after_the_data_is_gone() => Assert.Equal(["stop", "ingress", "data", "status", "release"], _calls);
    [Fact] void should_not_apply_anything() => _gateway.DidNotReceiveWithAnyArgs().Apply(default!, default);
}
#endif
