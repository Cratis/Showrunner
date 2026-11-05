// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_secret_that_matches : a_sign_in_callback
{
    int _status;

    async Task Because()
    {
        _options.SignInSecret = "shared-secret";
        _status = await Answer(Call(Body(), secret: "shared-secret"));
        await ReadOutbox();
    }

    [Fact] void should_accept_it() => _status.ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_tell_the_operator() => _outbox.Count.ShouldEqual(1);
}
#endif
