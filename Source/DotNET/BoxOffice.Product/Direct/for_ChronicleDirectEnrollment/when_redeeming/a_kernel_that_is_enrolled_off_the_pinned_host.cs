// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_redeeming;

public class a_kernel_that_is_enrolled_off_the_pinned_host : a_chronicle_direct_enrollment
{
    DirectRedemptionResult _result;

    async Task Because()
    {
        Status(Enrolled("https://elsewhere.example.net"));
        _result = await _enrollment.Redeem(Request(Code(), expectedHost: Host), CancellationToken.None);
    }

    [Fact] void should_not_allow_the_endpoint() => _result.Outcome.ShouldEqual(DirectRedemption.EndpointNotAllowed);
    [Fact] void should_report_no_endpoint() => _result.Endpoint.ShouldEqual(string.Empty);
}
#endif
