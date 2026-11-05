// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_sign_in_and_then_a_return_on_the_same_day : a_sign_in_callback
{
    int _first;
    int _second;

    async Task Because()
    {
        _first = await Answer(Call(Body("SignedIn"), "SignedIn"));
        _second = await Answer(Call(Body("Returned"), "Returned"));
        await ReadOutbox();
    }

    [Fact] void should_accept_both() => (_first, _second).ShouldEqual((204, 204));
    [Fact] void should_tell_the_operator_once() => _outbox.Count.ShouldEqual(1);
}
#endif
