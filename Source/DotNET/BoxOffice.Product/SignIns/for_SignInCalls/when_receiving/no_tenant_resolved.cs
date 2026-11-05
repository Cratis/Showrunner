// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class no_tenant_resolved : a_sign_in_callback
{
    int _status;

    async Task Because()
    {
        _status = await Answer(Call(Body(tenant: null)));
        await ReadOutbox();
    }

    [Fact] void should_accept_it() => _status.ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
