// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Metering.for_MeteringProblems.given;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering.for_BoxOfficeMeteringServiceCollectionExtensions.when_adding_metering;

public class with_a_meter_function : Specification
{
    ServiceCollection _services;
    ServiceProvider _provider;

    void Because()
    {
        _services = [];
        _services.AddLogging();
        _services.AddSingleton(Substitute.For<IMeteringStore>());
        _services.AddBoxOfficeMetering(options => options.CloseDelay = TimeSpan.FromHours(3));
        _services.AddMeter<SpecEvent>("compute", (_, _) => null);
        _provider = _services.BuildServiceProvider();
    }

    [Fact] void should_register_the_meter() => _provider.GetServices<IMeter>().Single().Id.ShouldEqual(new MeterId("compute"));
    [Fact] void should_measure_while_the_product_runs() => _services.ShouldContain(_ => _.ImplementationType == typeof(MeteringService));
    [Fact] void should_apply_the_options() => _provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<MeteringOptions>>().Value.CloseDelay.ShouldEqual(TimeSpan.FromHours(3));
    [Fact] void should_be_able_to_measure_a_tenant() => _provider.GetRequiredService<TenantMetering>().ShouldNotBeNull();
}
#endif
