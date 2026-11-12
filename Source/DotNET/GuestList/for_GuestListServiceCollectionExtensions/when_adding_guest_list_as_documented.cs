// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Querying;
using Cratis.Showrunner.GuestList.Receiving;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.for_GuestListServiceCollectionExtensions;

/// <summary>
/// Naming the product is enough: the rest has defaults that need no call to the operator.
/// </summary>
public class when_adding_guest_list_as_documented : Specification
{
    GuestListOptions _options;
    ServiceProvider _provider;

    void Because()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IChronicleClient>());
        services.AddSingleton(Options.Create(new ChronicleClientOptions()));
        services.AddGuestList(options => options.Product = "ledger");
        _provider = services.BuildServiceProvider();
        _options = _provider.GetRequiredService<IOptions<GuestListOptions>>().Value;
    }

    [Fact] void should_receive_from_direct_by_default() => _options.OperatorStores.Select(_ => _.Value).ShouldContainOnly("Direct");
    [Fact] void should_subscribe_for_the_product_by_default() => _options.SubscribeForProduct.ShouldBeTrue();
    [Fact] void should_declare_no_audiences_by_default() => _options.DeclaredAudiences.ShouldBeNull();
    [Fact] void should_have_the_guest_list_registered() => _provider.GetService<IGuestList>().ShouldBeOfExactType<GuestLists>();
    [Fact] void should_have_the_inbox_registered() => _provider.GetService<GuestListInbox>().ShouldNotBeNull();
}
#endif
