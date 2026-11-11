// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_notification_that_is_not_valid : a_sign_in_callback
{
    int _null;
    int _noSubject;
    int _blankSubject;
    int _whitespaceSubject;
    int _noProvider;
    int _blankProvider;
    int _unknownKind;

    async Task Because()
    {
        _null = await Answer(Call("null"));
        _noSubject = await Answer(Call("""{"kind":"SignedIn","identityProvider":"github","tenant":"fjord-logistics"}"""));
        _blankSubject = await Answer(Call(Body(subject: string.Empty), principal: string.Empty));
        _whitespaceSubject = await Answer(Call(Body(subject: "   "), principal: "   "));
        _noProvider = await Answer(Call("""{"kind":"SignedIn","subject":"1001","tenant":"fjord-logistics"}"""));
        _blankProvider = await Answer(Call("""{"kind":"SignedIn","subject":"1001","identityProvider":" ","tenant":"fjord-logistics"}"""));
        _unknownKind = await Answer(Call(Body("Visited")));
        await ReadOutbox();
    }

    [Fact] void should_refuse_a_null_body() => _null.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_refuse_a_missing_subject() => _noSubject.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_refuse_a_blank_subject() => _blankSubject.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_refuse_a_whitespace_subject() => _whitespaceSubject.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_refuse_a_missing_identity_provider() => _noProvider.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_refuse_a_blank_identity_provider() => _blankProvider.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_refuse_a_kind_that_is_not_sign_in_or_return() => _unknownKind.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
