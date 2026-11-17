// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class a_closed_link : a_direct_page
{
    Answer _get;
    Answer _post;

    async Task Because()
    {
        Redeems();
        Connects(DirectLinkHealth.Healthy);
        var page = await Open();
        await Send(Form(Administrator, page.Token, Code));
        _get = await Open();
        _post = await Send(Form(Administrator, page.Token, Code));
    }

    [Fact] void should_answer_not_found_to_a_get() => _get.Status.ShouldEqual(StatusCodes.Status404NotFound);
    [Fact] void should_answer_not_found_to_a_post() => _post.Status.ShouldEqual(StatusCodes.Status404NotFound);
    [Fact] void should_answer_with_a_page() => _get.Headers.ContentType.ToString().StartsWith("text/html", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_not_carry_a_form_token() => _get.Token.ShouldBeEmpty();
    [Fact] void should_have_redeemed_only_once() => _enrollment.Received(1).Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
