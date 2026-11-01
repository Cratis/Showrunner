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
    [Fact] void should_accept_nothing_configured_when_switched_off() => Resolve(options => options.Enabled = false).Enabled.ShouldBeFalse();
}
#endif
