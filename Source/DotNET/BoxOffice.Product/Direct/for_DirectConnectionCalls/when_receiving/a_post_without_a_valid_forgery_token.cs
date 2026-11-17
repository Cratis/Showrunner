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
        var forSomeoneElse = _forgery.TokenFor("2002");
        var tampered = page.Token[..^1] + (page.Token[^1] == '0' ? '1' : '0');
        _answers =
        [
            await Send(Form(Administrator, null, Code)),
            await Send(Form(Administrator, "ABCDEF", Code)),
            await Send(Form(Administrator, "99999999999999999999.AB", Code)),
            await Send(Form(Administrator, tampered, Code)),
            await Send(Form(Administrator, forSomeoneElse, Code))
        ];
    }

    [Fact] void should_refuse_every_one() => _answers.ShouldEachConformTo(answer => answer.Status == StatusCodes.Status400BadRequest);
    [Fact] void should_say_the_form_had_expired() => _answers.ShouldEachConformTo(answer => answer.Html.Contains("This form had expired"));
    [Fact] void should_carry_a_fresh_form() => _answers.ShouldEachConformTo(answer => _forgery.IsValid(answer.Token, Administrator));
    [Fact] void should_answer_with_a_page() => _answers.ShouldEachConformTo(answer => answer.Headers.ContentType.ToString().StartsWith("text/html", StringComparison.Ordinal));
    [Fact] void should_not_redeem_a_code() => _enrollment.DidNotReceive().Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
