// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class a_body_that_is_too_long : a_direct_page
{
    Answer _answer;

    async Task Because()
    {
        var page = await Open();
        _answer = await Send(Form(Administrator, page.Nonce, page.Token, new string('a', DirectConnectionCalls.MaximumBodyLength)));
    }

    [Fact] void should_refuse_it() => _answer.Status.ShouldEqual(StatusCodes.Status400BadRequest);
}
#endif
