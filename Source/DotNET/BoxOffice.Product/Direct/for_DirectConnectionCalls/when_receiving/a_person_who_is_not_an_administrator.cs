// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class a_person_who_is_not_an_administrator : a_direct_page
{
    Answer _get;
    Answer _post;

    async Task Because()
    {
        _get = await Open("2002");
        _post = await Send(Form("2002", "nonce", _forgery.TokenFor("nonce", "2002"), Code));
    }

    [Fact] void should_refuse_the_page() => _get.Status.ShouldEqual(StatusCodes.Status403Forbidden);
    [Fact] void should_refuse_the_post_even_with_a_valid_token() => _post.Status.ShouldEqual(StatusCodes.Status403Forbidden);
    [Fact] void should_not_redeem_a_code() => _enrollment.DidNotReceive().Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
