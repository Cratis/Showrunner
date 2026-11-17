// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class a_cross_site_post : a_direct_page
{
    Answer _answer;

    async Task Because()
    {
        Redeems();
        Connects(DirectLinkHealth.Healthy);
        var page = await Open();
        _answer = await Send(Form(Administrator, page.Token, Code, site: "cross-site"));
    }

    [Fact] void should_refuse_it() => _answer.Status.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_say_why() => _answer.Html.ShouldContain("sent from another site");
    [Fact] void should_not_redeem_a_code() => _enrollment.DidNotReceive().Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
