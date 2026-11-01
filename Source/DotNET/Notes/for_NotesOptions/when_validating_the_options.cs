// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.for_NotesOptions;

public class when_validating_the_options : Specification
{
    static NotesOptions Resolve(Action<NotesOptions> configure)
    {
        var services = new ServiceCollection();
        services.AddNotes(configure);
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<NotesOptions>>().Value;
    }

    static void Complete(NotesOptions options)
    {
        options.ApplicationName = "Ledger";
        options.PublicationId = "pub-7f3a";
    }

    static void Http(NotesOptions options, string url = "https://studio.internal:8443", string token = "")
    {
        Complete(options);
        options.Transport = NotesTransport.Http;
        options.CollectorUrl = url;
        options.CollectorToken = token;
    }

    [Fact] void should_accept_a_complete_configuration() => Resolve(Complete).Enabled.ShouldBeTrue();
    [Fact] void should_default_the_collecting_store_to_studio() => Resolve(Complete).CollectingStores.Select(_ => _.Value).ShouldContainOnly("Studio");
    [Fact] void should_default_to_polling_every_minute() => Resolve(Complete).PollingInterval.ShouldEqual(TimeSpan.FromMinutes(1));
    [Fact] void should_refuse_a_missing_application_name() => Assert.Throws<OptionsValidationException>(() => Resolve(options => options.PublicationId = "pub-7f3a"));
    [Fact] void should_refuse_a_missing_publication() => Assert.Throws<OptionsValidationException>(() => Resolve(options => options.ApplicationName = "Ledger"));
    [Fact] void should_refuse_no_collecting_store() => Assert.Throws<OptionsValidationException>(() => Resolve(options =>
    {
        Complete(options);
        options.CollectingStores = [];
    }));
    [Fact] void should_refuse_a_polling_interval_of_zero() => Assert.Throws<OptionsValidationException>(() => Resolve(options =>
    {
        Complete(options);
        options.PollingInterval = TimeSpan.Zero;
    }));
    [Fact] void should_use_the_same_kernel_transport_by_default() => Resolve(Complete).Transport.ShouldEqual(NotesTransport.SameKernel);
    [Fact] void should_accept_the_http_transport_with_a_url_and_a_secret() => Resolve(options => Http(options, token: "secret")).Transport.ShouldEqual(NotesTransport.Http);
    [Fact] void should_not_need_a_collecting_store_over_http() => Resolve(options =>
    {
        Http(options, token: "secret");
        options.CollectingStores = [];
    }).Transport.ShouldEqual(NotesTransport.Http);
    [Fact] void should_refuse_http_without_a_url() => Assert.Throws<OptionsValidationException>(() => Resolve(options => Http(options, url: "", token: "secret")));
    [Fact] void should_refuse_http_with_a_relative_url() => Assert.Throws<OptionsValidationException>(() => Resolve(options => Http(options, url: "/notes", token: "secret")));
    [Fact] void should_refuse_http_with_another_scheme() => Assert.Throws<OptionsValidationException>(() => Resolve(options => Http(options, url: "ftp://studio.internal", token: "secret")));
    [Fact] void should_refuse_http_without_a_secret() => Assert.Throws<OptionsValidationException>(() => Resolve(options => Http(options, token: "")));
    [Fact] void should_refuse_http_with_a_secret_file_that_does_not_exist() => Assert.Throws<OptionsValidationException>(() => Resolve(options =>
    {
        Http(options, token: "");
        options.CollectorTokenFile = "/no/such/secret";
    }));
    [Fact] void should_refuse_a_batch_size_of_zero() => Assert.Throws<OptionsValidationException>(() => Resolve(options =>
    {
        Http(options, token: "secret");
        options.RelayBatchSize = 0;
    }));
    [Fact] void should_not_check_the_collector_when_the_transport_is_the_same_kernel() => Resolve(options =>
    {
        Complete(options);
        options.CollectorUrl = "not a url";
    }).Transport.ShouldEqual(NotesTransport.SameKernel);
    [Fact] void should_accept_nothing_configured_when_switched_off() => Resolve(options => options.Enabled = false).Enabled.ShouldBeFalse();
}
#endif
