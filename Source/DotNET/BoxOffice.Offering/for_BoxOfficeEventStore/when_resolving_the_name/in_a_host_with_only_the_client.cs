// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Offering.for_BoxOfficeEventStore.when_resolving_the_name;

public class in_a_host_with_only_the_client : Specification
{
    EventStoreName _name;

    void Because()
    {
        var services = new ServiceCollection();

        using var provider = services.BuildServiceProvider();
        _name = new BoxOfficeEventStore(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new ChronicleClientOptions { EventStore = "Ledger" }),
            Options.Create(new OfferingOptions())).Name;
    }

    [Fact] void should_be_the_name_in_the_client_options() => Assert.Equal(new EventStoreName("Ledger"), _name);
}
#endif
