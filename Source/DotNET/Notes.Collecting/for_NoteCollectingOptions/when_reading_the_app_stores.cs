// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Collecting.for_NoteCollectingOptions;

public class when_reading_the_app_stores : Specification
{
    IEnumerable<EventStoreName> _stores;

    async Task Because() => _stores = await new ConfiguredAppStores(Options.Create(new NoteCollectingOptions { AppStores = ["Ledger", "Shop"] })).Get();

    [Fact] void should_name_the_configured_stores() => _stores.Select(_ => _.Value).ShouldContainOnly("Ledger", "Shop");
}
#endif
