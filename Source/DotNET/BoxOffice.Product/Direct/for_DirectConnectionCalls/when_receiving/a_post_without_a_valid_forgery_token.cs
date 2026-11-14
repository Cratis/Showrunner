// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class a_post_without_a_valid_forgery_token : a_direct_page
{
    Answer[] _answers;

    async Task Because()
    {
        Redeems();
        Connects(DirectLinkHealth.Healthy);
        var page = await Open();
        _answers =
        [
            await Send(Form(Administrator, page.Nonce, null, Code)),
            await Send(Form(Administrator, page.Nonce, "ABCDEF", Code)),
            await Send(Form(Administrator, null, page.Token, Code)),
            await Send(Form(Administrator, "another-nonce", page.Token, Code)),
            await Send(Form(Administrator, page.Nonce, _forgery.TokenFor(page.Nonce, "2002"), Code))
        ];
    }

    [Fact] void should_refuse_every_one() => _answers.ShouldEachConformTo(answer => answer.Status == StatusCodes.Status400BadRequest);
    [Fact] void should_not_redeem_a_code() => _enrollment.DidNotReceive().Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
