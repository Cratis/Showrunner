// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class a_post_after_the_form_expired : a_direct_page
{
    Answer _expired;
    Answer _retried;

    async Task Because()
    {
        Redeems();
        Connects(DirectLinkHealth.Healthy);
        var page = await Open();
        _clock.Now += DirectForgery.Lifetime + TimeSpan.FromSeconds(1);
        _expired = await Send(Form(Administrator, page.Token, Code));
        _retried = await Send(Form(Administrator, _expired.Token, Code));
    }

    [Fact] void should_refuse_it() => _expired.Status.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_say_the_form_had_expired() => _expired.Html.ShouldContain("This form had expired");
    [Fact] void should_ask_for_the_code_again() => _expired.Html.ShouldContain("name=\"code\"");
    [Fact] void should_connect_with_the_fresh_form() => _retried.Html.ShouldContain("The product is connected to Chronicle");
    [Fact] void should_redeem_only_once() => _enrollment.Received(1).Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
