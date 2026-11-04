// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_secret_that_does_not_match : a_sign_in_callback
{
    int _wrong;
    int _missing;

    async Task Because()
    {
        _options.SignInSecret = "shared-secret";
        _wrong = await Answer(Call(Body(), secret: "another-secret"));
        _missing = await Answer(Call(Body()));
        await ReadOutbox();
    }

    [Fact] void should_refuse_a_wrong_secret() => _wrong.ShouldEqual(StatusCodes.Status401Unauthorized);
    [Fact] void should_refuse_a_missing_secret() => _missing.ShouldEqual(StatusCodes.Status401Unauthorized);
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
