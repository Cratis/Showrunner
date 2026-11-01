// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Collecting.for_NoteCollectingOptions;

public class when_validating_the_options : Specification
{
    static NoteCollectingOptions Resolve(Action<NoteCollectingOptions> configure)
    {
        var services = new ServiceCollection();
        services.AddNoteCollecting(configure);
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<NoteCollectingOptions>>().Value;
    }

    [Fact] void should_accept_no_configured_stores_because_the_host_can_name_them_itself() => Resolve(_ => { }).AppStores.ShouldBeEmpty();
    [Fact] void should_default_to_polling_every_minute() => Resolve(_ => { }).PollingInterval.ShouldEqual(TimeSpan.FromMinutes(1));
    [Fact] void should_refuse_a_polling_interval_of_zero() => Assert.Throws<OptionsValidationException>(() => Resolve(options => options.PollingInterval = TimeSpan.Zero));
}
#endif
