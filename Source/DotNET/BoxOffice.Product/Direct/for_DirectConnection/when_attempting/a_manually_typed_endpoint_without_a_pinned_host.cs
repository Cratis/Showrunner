// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.when_attempting;

public class a_manually_typed_endpoint_without_a_pinned_host : a_direct_connection
{
    DirectAttemptResult _result;

    async Task Because()
    {
        Redeems();
        _result = await _connection.Attempt(Code, "https://attacker.example.net", CancellationToken.None);
    }

    [Fact] void should_refuse_the_endpoint() => _result.ShouldEqual(DirectAttemptResult.EndpointRefused);
    [Fact] void should_not_redeem_the_code() => _enrollment.DidNotReceive().Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
