// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Cratis.Chronicle.Transactions;
using Cratis.Showrunner.BoxOffice.Product.Access;
using Cratis.Showrunner.BoxOffice.Product.Activity;
using Cratis.Showrunner.BoxOffice.Product.Licensing;
using Cratis.Showrunner.BoxOffice.Product.Provisioning;
using Cratis.Showrunner.BoxOffice.Product.SignIns;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.for_BoxOfficeProductServiceCollectionExtensions.when_adding_the_product_part;

public class to_a_cratis_host : Specification
{
    ISeatLicenses _licenses;
    ITenantAccess _access;
    IReportActivity _activity;
    bool _hasWorker;
    bool _hasStartupFilter;
    IEnumerable<EventStoreName> _operators;
    BoxOfficeProductOptions _options;
    Exception _invalid;

    void Because()
    {
        var applicationStore = Substitute.For<IEventStore>();
        applicationStore.Name.Returns(new EventStoreName("Ledger"));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IChronicleClient>());
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ChronicleClientOptions()));
        services.AddSingleton(Substitute.For<IReadModels>());
        services.AddSingleton(Substitute.For<IUnitOfWorkManager>());
        services.AddScoped(_ => applicationStore);
        services.AddSingleton(Substitute.For<IProvisionTenants>());
        services.AddBoxOfficeProduct(product => product.ConfigureBoxOffice(box => box.OperatorStores = ["Direct", "Studio"]));
        using var provider = services.BuildServiceProvider();
        _licenses = provider.GetRequiredService<ISeatLicenses>();
        _access = provider.GetRequiredService<ITenantAccess>();
        _activity = provider.GetRequiredService<IReportActivity>();
        _hasWorker = provider.GetServices<IHostedService>().OfType<BoxOfficeProductWorker>().Any();
        _hasStartupFilter = provider.GetServices<IStartupFilter>().OfType<SignInCallbackStartupFilter>().Any();
        _operators = provider.GetRequiredService<IOptions<BoxOfficeOptions>>().Value.OperatorStores;
        _options = provider.GetRequiredService<IOptions<BoxOfficeProductOptions>>().Value;

        var invalid = new ServiceCollection();
        invalid.AddLogging();
        invalid.AddBoxOfficeProduct(product => product.SignInPath = "sign-in");
        using var invalidProvider = invalid.BuildServiceProvider();
        _invalid = Catch.Exception(() => _ = invalidProvider.GetRequiredService<IOptions<BoxOfficeProductOptions>>().Value);
    }

    [Fact] void should_provide_the_seat_licenses() => _licenses.ShouldBeOfExactType<SeatLicenses>();
    [Fact] void should_provide_the_tenant_access() => _access.ShouldBeOfExactType<TenantAccess>();
    [Fact] void should_provide_the_activity_reporting() => _activity.ShouldBeOfExactType<ActivityReporter>();
    [Fact] void should_listen_for_the_tenants_the_operator_registers() => _hasWorker.ShouldBeTrue();
    [Fact] void should_receive_the_sign_in_callback_without_mapping_an_endpoint() => _hasStartupFilter.ShouldBeTrue();
    [Fact] void should_configure_box_office_through_the_product_options() => _operators.Select(_ => _.Value).ShouldContainOnly(["Direct", "Studio"]);
    [Fact] void should_receive_the_sign_in_callback_at_the_default_path() => _options.SignInPath.ShouldEqual("/.cratis/showrunner/sign-in");
    [Fact] void should_refuse_a_sign_in_path_that_is_not_a_path() => _invalid.ShouldBeOfExactType<OptionsValidationException>();
}
#endif
