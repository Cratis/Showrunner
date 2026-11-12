// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.given;
using Cratis.Showrunner.GuestList.Querying.for_GuestLists.given;

namespace Cratis.Showrunner.GuestList.Querying.for_GuestLists.when_asking;

/// <summary>
/// Nobody is in an audience nobody granted: unknown is empty, never an error.
/// </summary>
public class when_nothing_was_recorded : a_guest_list
{
    IReadOnlySet<AudienceName> _audiences;
    IReadOnlyList<GrantedAudience> _granted;

    async Task Because()
    {
        _audiences = await _guestList.For(Guests.Acme, Guests.Alice);
        _granted = await _guestList.Granted(Guests.Acme);
    }

    [Fact] void should_have_no_audiences() => _audiences.ShouldBeEmpty();
    [Fact] void should_have_no_grants() => _granted.ShouldBeEmpty();
}
#endif
