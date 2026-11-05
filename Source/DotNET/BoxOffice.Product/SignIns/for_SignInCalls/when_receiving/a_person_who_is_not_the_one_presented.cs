// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_person_who_is_not_the_one_presented : a_sign_in_callback
{
    int _other;
    int _none;

    async Task Because()
    {
        _other = await Answer(Call(Body(subject: "1002")));
        _none = await Answer(Call(Body(), principal: null));
        await ReadOutbox();
    }

    [Fact] void should_refuse_a_call_for_someone_else() => _other.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_refuse_a_call_with_nobody_presented() => _none.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
