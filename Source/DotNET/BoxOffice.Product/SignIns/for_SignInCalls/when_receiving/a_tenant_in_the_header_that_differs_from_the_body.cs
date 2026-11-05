// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_tenant_in_the_header_that_differs_from_the_body : a_sign_in_callback
{
    int _status;

    async Task Because()
    {
        _status = await Answer(Call(Body(), tenantHeader: "somebody-elses"));
        await ReadOutbox();
    }

    [Fact] void should_refuse_it() => _status.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
