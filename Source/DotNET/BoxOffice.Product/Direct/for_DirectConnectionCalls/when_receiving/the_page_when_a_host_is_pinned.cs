// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class the_page_when_a_host_is_pinned : a_direct_page
{
    Answer _answer;

    async Task Because()
    {
        _options.Direct.ExpectedEndpointHost = "chronicle.example.com";
        _answer = await Open();
    }

    [Fact] void should_let_a_person_type_an_endpoint() => _answer.Html.ShouldContain("name=\"endpoint\"");
}
#endif
