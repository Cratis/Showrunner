// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.when_attempting;

public class a_hostile_endpoint_with_a_pinned_host : a_direct_connection
{
    DirectAttemptResult[] _results;

    async Task Because()
    {
        _options.Direct.ExpectedEndpointHost = "chronicle.example.com";
        Redeems();
        string[] hostile =
        [
            "https://attacker.example.net",
            "http://chronicle.example.com",
            "https://chronicle.example.com@attacker.example.net",
            "https://user:secret@chronicle.example.com",
            "https://chronicle.example.com.attacker.example.net",
            "not a url"
        ];
        _results = [.. await Task.WhenAll(hostile.Select(endpoint => _connection.Attempt(Code, endpoint, CancellationToken.None)))];
    }

    [Fact] void should_refuse_every_one() => _results.ShouldEachConformTo(result => result == DirectAttemptResult.EndpointRefused);
    [Fact] void should_not_redeem_any_code() => _enrollment.DidNotReceive().Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
