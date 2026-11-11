// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_body_at_and_over_the_length_limit : a_sign_in_callback
{
    int _declaredTooLong;
    int _streamedTooLong;
    int _exactlyAtTheLimit;
    int _outboxAfterRefusals;

    async Task Because()
    {
        var declared = Call(Body());
        declared.Request.ContentLength = SignInCalls.MaximumBodyLength + 1;
        _declaredTooLong = await Answer(declared);

        _streamedTooLong = await Answer(Call(Body() + new string(' ', SignInCalls.MaximumBodyLength)));
        await ReadOutbox();
        _outboxAfterRefusals = _outbox.Count;

        var body = Body();
        _exactlyAtTheLimit = await Answer(Call(body + new string(' ', SignInCalls.MaximumBodyLength - body.Length)));
        await ReadOutbox();
    }

    [Fact] void should_refuse_a_declared_length_over_the_limit() => _declaredTooLong.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_refuse_a_streamed_body_over_the_limit() => _streamedTooLong.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_tell_the_operator_nothing_for_the_refusals() => _outboxAfterRefusals.ShouldEqual(0);
    [Fact] void should_accept_a_body_exactly_at_the_limit() => _exactlyAtTheLimit.ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_tell_the_operator_for_the_one_at_the_limit() => _outbox.Count.ShouldEqual(1);
}
#endif
