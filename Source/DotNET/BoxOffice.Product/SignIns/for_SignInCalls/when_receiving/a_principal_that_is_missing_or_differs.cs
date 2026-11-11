// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_principal_that_is_missing_or_differs : a_sign_in_callback
{
    int _missing;
    int _differentCase;
    int _trailingSpace;
    int _differentValue;

    async Task Because()
    {
        _missing = await Answer(Call(Body(), principal: null));
        _differentCase = await Answer(Call(Body(subject: "ABC"), principal: "abc"));
        _trailingSpace = await Answer(Call(Body(subject: "1001"), principal: "1001 "));
        _differentValue = await Answer(Call(Body(), principal: "1002"));
        await ReadOutbox();
    }

    [Fact] void should_refuse_a_call_without_a_principal() => _missing.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_compare_the_case() => _differentCase.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_compare_a_trailing_space() => _trailingSpace.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_refuse_another_person() => _differentValue.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
