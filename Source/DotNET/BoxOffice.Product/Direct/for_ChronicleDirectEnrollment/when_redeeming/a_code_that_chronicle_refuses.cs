// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.Federation;
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_redeeming;

public class a_code_that_chronicle_refuses : a_chronicle_direct_enrollment
{
    DirectRedemptionResult _result;

    async Task Because()
    {
        _federation.Redeem(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns<FederationRedemption>(_ => throw new InvalidOperationException($"refused {Secret}"));
        _result = await _enrollment.Redeem(Request(Code()), CancellationToken.None);
    }

    [Fact] void should_be_refused() => _result.Outcome.ShouldEqual(DirectRedemption.Refused);
    [Fact] void should_report_no_endpoint() => _result.Endpoint.ShouldEqual(string.Empty);
    [Fact] void should_never_log_the_code() => Logged().Any(text => text.Contains(Secret)).ShouldBeFalse();
}
#endif
