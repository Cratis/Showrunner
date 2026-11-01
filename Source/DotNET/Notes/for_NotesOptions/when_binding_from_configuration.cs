// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.Configuration;

namespace Cratis.Showrunner.Notes.for_NotesOptions;

public class when_binding_from_configuration : Specification
{
    NotesOptions _options;

    void Because() => _options = new NotesOptions().BindFrom(new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Notes:Transport"] = "Http",
            ["Notes:ApplicationName"] = "Ledger",
            ["Notes:PublicationId"] = "pub-7f3a",
            ["Notes:ApplicationVersion"] = "1.4.0",
            ["Notes:CollectorTenantId"] = "acme",
            ["Notes:CollectorUrl"] = "https://collector.example.invalid",
            ["Notes:CollectingStores:0"] = "Collector",
            ["Notes:RelayBatchSize"] = "25"
        })
        .Build()
        .GetSection("Notes"));

    [Fact] void should_bind_the_application_name() => _options.ApplicationName.Value.ShouldEqual("Ledger");
    [Fact] void should_bind_the_publication() => _options.PublicationId.Value.ShouldEqual("pub-7f3a");
    [Fact] void should_bind_the_application_version() => _options.ApplicationVersion.Value.ShouldEqual("1.4.0");
    [Fact] void should_bind_the_collector_tenant() => _options.CollectorTenantId.Value.ShouldEqual("acme");
    [Fact] void should_bind_the_collecting_stores() => _options.CollectingStores.Select(_ => _.Value).ShouldContainOnly("Collector");
    [Fact] void should_bind_the_transport() => _options.Transport.ShouldEqual(NotesTransport.Http);
    [Fact] void should_bind_the_collector_url() => _options.CollectorUrl.ShouldEqual("https://collector.example.invalid");
    [Fact] void should_bind_plain_values() => _options.RelayBatchSize.ShouldEqual(25);
}
#endif
