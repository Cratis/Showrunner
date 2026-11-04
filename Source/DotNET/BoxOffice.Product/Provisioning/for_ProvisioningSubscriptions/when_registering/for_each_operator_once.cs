// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.Reactors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_ProvisioningSubscriptions.when_registering;

public class for_each_operator_once : Specification
{
    IEventStore _store;
    IReactors _reactors;

    async Task Because()
    {
        _reactors = Substitute.For<IReactors>();
        _store = Substitute.For<IEventStore>();
        _store.Reactors.Returns(_reactors);
        var subscriptions = new ProvisioningSubscriptions(
            new ServiceCollection().BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new BoxOfficeOptions { OperatorStores = ["Direct", "Studio"] }));
        await subscriptions.Register(_store);
        await subscriptions.Register(_store);
    }

    [Fact] void should_listen_to_the_first_operator_once() => _reactors.Received(1).Register(ProvisioningSubscriptions.ReactorFor("Direct"), Arg.Any<Action<IReactorBuilder>>());
    [Fact] void should_listen_to_the_second_operator_once() => _reactors.Received(1).Register(ProvisioningSubscriptions.ReactorFor("Studio"), Arg.Any<Action<IReactorBuilder>>());
}
#endif
