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
/// An operator that does not address decisions to products leaves the routing audience off.
/// </summary>
public class when_not_addressing_to_products : a_guest_list_outbox
{
    async Task Because()
    {
        _outbox = new(_client, Substitute.For<ILogger<GuestListOutbox>>(), Options.Create(new GuestListAuthoringOptions { AddressToProducts = false }));
        await Happened(Guests.TenantGrant);
        await ReadPublished();
    }

    [Fact] void should_not_address_the_decision() => _published.Single().Context.NamedTags.ShouldBeEmpty();
}
#endif
