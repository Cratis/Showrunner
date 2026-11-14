// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_redeeming;

public class a_kernel_that_is_enrolled_already : a_chronicle_direct_enrollment
{
    DirectRedemptionResult _result;

    async Task Because()
    {
        Status(Enrolled());
        _result = await _enrollment.Redeem(Request(Code(), expectedHost: Host), CancellationToken.None);
    }

    [Fact] void should_be_reported_as_redeemed() => _result.Outcome.ShouldEqual(DirectRedemption.Redeemed);
    [Fact] void should_report_the_endpoint_of_the_enrollment() => _result.Endpoint.ShouldEqual(Endpoint);
    [Fact] void should_leave_the_code_unspent() => _federation.DidNotReceive().Redeem(Arg.Any<string>(), Arg.Any<CancellationToken>());
}
#endif
