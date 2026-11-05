// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class the_same_sign_in_twice : a_sign_in_callback
{
    async Task Because()
    {
        await Answer(Call(Body()));
        await Answer(Call(Body()));
        await ReadOutbox();
    }

    [Fact] void should_tell_the_operator_once() => _outbox.Count.ShouldEqual(1);
}
#endif
