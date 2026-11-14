// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_redeeming;

public class an_expired_enrollment : a_chronicle_direct_enrollment
{
    DirectRedemptionResult _result;

    async Task Because()
    {
        Status(Enrolled(expired: true));
        _federation.Redeem(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Redemption());
        _result = await _enrollment.Redeem(Request(Code()), CancellationToken.None);
    }

    [Fact] void should_be_redeemed() => _result.Outcome.ShouldEqual(DirectRedemption.Redeemed);
    [Fact] void should_use_the_new_code() => _federation.Received(1).Redeem(Code(), Arg.Any<CancellationToken>());
}
#endif
