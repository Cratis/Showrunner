// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_redeeming;

public class a_typed_endpoint_that_matches_the_code : a_chronicle_direct_enrollment
{
    DirectRedemptionResult _result;

    async Task Because()
    {
        _federation.Redeem(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Redemption());
        _result = await _enrollment.Redeem(Request(Code(), Endpoint, Host), CancellationToken.None);
    }

    [Fact] void should_be_redeemed() => _result.Outcome.ShouldEqual(DirectRedemption.Redeemed);
}
#endif
