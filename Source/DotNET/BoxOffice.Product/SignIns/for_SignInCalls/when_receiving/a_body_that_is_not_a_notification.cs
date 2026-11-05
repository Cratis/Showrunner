// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_body_that_is_not_a_notification : a_sign_in_callback
{
    int _garbage;
    int _empty;
    int _tooLong;

    async Task Because()
    {
        _garbage = await Answer(Call("not json"));
        _empty = await Answer(Call(string.Empty));
        _tooLong = await Answer(Call(new string(' ', SignInCalls.MaximumBodyLength + 1)));
        await ReadOutbox();
    }

    [Fact] void should_refuse_what_is_not_json() => _garbage.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_refuse_an_empty_body() => _empty.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_refuse_a_body_that_is_too_long() => _tooLong.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
