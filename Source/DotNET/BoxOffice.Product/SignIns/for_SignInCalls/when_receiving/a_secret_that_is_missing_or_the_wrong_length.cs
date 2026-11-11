// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_secret_that_is_missing_or_the_wrong_length : a_sign_in_callback
{
    int _missing;
    int _tooShort;
    int _tooLong;

    async Task Because()
    {
        _options.SignInSecret = "shared-secret";
        _missing = await Answer(Call(Body()));
        _tooShort = await Answer(Call(Body(), secret: "shared"));
        _tooLong = await Answer(Call(Body(), secret: "shared-secret-and-more"));
        await ReadOutbox();
    }

    [Fact] void should_refuse_a_call_without_it() => _missing.ShouldEqual(StatusCodes.Status401Unauthorized);
    [Fact] void should_refuse_a_shorter_one() => _tooShort.ShouldEqual(StatusCodes.Status401Unauthorized);
    [Fact] void should_refuse_a_longer_one() => _tooLong.ShouldEqual(StatusCodes.Status401Unauthorized);
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
