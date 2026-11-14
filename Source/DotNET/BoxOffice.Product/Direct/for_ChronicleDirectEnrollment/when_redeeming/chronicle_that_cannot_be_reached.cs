// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.Federation;
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_redeeming;

public class chronicle_that_cannot_be_reached : a_chronicle_direct_enrollment
{
    DirectRedemptionResult _result;

    async Task Because()
    {
        StatusFails();
        _federation.Redeem(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns<FederationRedemption>(_ => throw new HttpRequestException($"could not reach with {Secret}"));
        _result = await _enrollment.Redeem(Request(Code()), CancellationToken.None);
    }

    [Fact] void should_be_unreachable() => _result.Outcome.ShouldEqual(DirectRedemption.Unreachable);
    [Fact] void should_never_log_what_the_failure_said() => Logged().Any(text => text.Contains(Secret)).ShouldBeFalse();
}
#endif
