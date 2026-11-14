// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_redeeming;

public class a_valid_code : a_chronicle_direct_enrollment
{
    DirectRedemptionResult _result;

    async Task Because()
    {
        _federation.Redeem(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Redemption());
        _result = await _enrollment.Redeem(Request(Code(), expectedHost: Host), CancellationToken.None);
    }

    [Fact] void should_be_redeemed() => _result.Outcome.ShouldEqual(DirectRedemption.Redeemed);
    [Fact] void should_report_the_endpoint_the_kernel_enrolled_with() => _result.Endpoint.ShouldEqual(Endpoint);
    [Fact] void should_redeem_the_code_once() => _federation.Received(1).Redeem(Code(), Arg.Any<CancellationToken>());
    [Fact] void should_never_log_the_code() => Logged().Any(text => text.Contains(Secret)).ShouldBeFalse();
}
#endif
