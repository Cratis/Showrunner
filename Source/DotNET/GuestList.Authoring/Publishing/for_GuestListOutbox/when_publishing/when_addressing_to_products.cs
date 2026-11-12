// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;
using Cratis.Showrunner.GuestList.Authoring.Publishing.for_GuestListOutbox.given;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Authoring.Publishing.for_GuestListOutbox.when_publishing;

/// <summary>
/// A decision is addressed to its product with Chronicle's routing audience, a label of the event and not a field of it.
/// </summary>
public class when_addressing_to_products : a_guest_list_outbox
{
    async Task Because()
    {
        _outbox = new(_client, Substitute.For<ILogger<GuestListOutbox>>(), Options.Create(new GuestListAuthoringOptions { AddressToProducts = true }));
        await Happened(Guests.TenantGrant);
        await ReadPublished();
    }

    [Fact] void should_address_the_decision_to_its_product() => _published.Single().Context.NamedTags.ShouldContain(new NamedTag(WellKnownTags.Audience, Guests.Product.Value));
}
#endif
