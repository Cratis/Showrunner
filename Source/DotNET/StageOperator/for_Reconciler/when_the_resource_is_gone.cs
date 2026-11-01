// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.Kubernetes;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.StageOperator.for_Reconciler;

public class when_the_resource_is_gone : Specification
{
    IKubernetesGateway _gateway = null!;
    TimeSpan? _result;

    void Establish()
    {
        _gateway = Substitute.For<IKubernetesGateway>();
        _gateway.GetApplication("published", "orders", Arg.Any<CancellationToken>()).Returns((StageApplication?)null);
    }

    async Task Because() => _result = await new Reconciler(_gateway, Options.Create(new StageOperatorOptions()), TimeProvider.System, NullLogger<Reconciler>.Instance).Reconcile("published", "orders", CancellationToken.None);

    [Fact] void should_not_ask_to_be_called_again() => Assert.Null(_result);
    [Fact] void should_touch_nothing() => _gateway.DidNotReceiveWithAnyArgs().Observe(default!, default);
}
#endif
