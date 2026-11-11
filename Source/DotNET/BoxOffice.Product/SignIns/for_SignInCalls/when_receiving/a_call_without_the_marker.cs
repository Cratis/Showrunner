// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_call_without_the_marker : a_sign_in_callback
{
    DefaultHttpContext _http;
    int _status;

    async Task Because()
    {
        _http = Call(Body(), marker: null);
        _status = await Answer(_http);
        await ReadOutbox();
    }

    [Fact] void should_refuse_it() => _status.ShouldEqual(StatusCodes.Status401Unauthorized);
    [Fact] void should_not_let_the_answer_be_cached() => _http.Response.Headers.CacheControl.ToString().ShouldEqual("no-store");
    [Fact] void should_tell_the_operator_nothing() => _outbox.ShouldBeEmpty();
}
#endif
