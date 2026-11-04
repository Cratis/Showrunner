// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_sign_in : a_sign_in_callback
{
    int _status;

    async Task Because()
    {
        _status = await Answer(Call(Body()));
        await ReadOutbox();
    }

    [Fact] void should_accept_it() => _status.ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_tell_the_operator_who_was_active() => _outbox.Single().Content.ShouldEqual(new SeatHolderActive(Tenants.Tenant, Tenants.Person("1001", "Ada Lovelace", "ada@fjord.example.com"), Tenants.Today));
}
#endif
