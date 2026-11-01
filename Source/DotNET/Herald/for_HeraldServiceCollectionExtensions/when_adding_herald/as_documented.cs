// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Sending;
using Cratis.Showrunner.Herald.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.for_HeraldServiceCollectionExtensions.when_adding_herald;

public class as_documented : Specification
{
    string _folder;
    HeraldOptions _options;
    IEmailTransport _transport;
    IEmails _emails;

    void Because()
    {
        _folder = Directory.CreateTempSubdirectory("herald-").FullName;
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<Cratis.Chronicle.IChronicleClient>());
        services.AddSingleton(Options.Create(new Cratis.Chronicle.ChronicleClientOptions()));
        services.AddHerald(options =>
        {
            options.TemplatesFolder = _folder;
            options.Sender.Address = "noreply@ledger.example.com";
            options.Smtp.Host = "smtp.example.com";
        });
        using var provider = services.BuildServiceProvider();
        _options = provider.GetRequiredService<IOptions<HeraldOptions>>().Value;
        _transport = provider.GetRequiredService<IEmailTransport>();
        _emails = provider.GetRequiredService<IEmails>();
    }

    void Destroy() => Directory.Delete(_folder);

    [Fact] void should_accept_the_options() => Assert.Equal("noreply@ledger.example.com", _options.Sender.Address);
    [Fact] void should_send_over_smtp_by_default() => Assert.IsType<SmtpEmailTransport>(_transport);
    [Fact] void should_provide_the_service_to_send_with() => Assert.IsType<Emails>(_emails);
}
#endif
