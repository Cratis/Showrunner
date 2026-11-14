// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_redeeming;

public class a_typed_endpoint_that_differs_from_the_code : a_chronicle_direct_enrollment
{
    DirectRedemptionResult _result;

    async Task Because() =>
        _result = await _enrollment.Redeem(Request(Code(), "https://chronicle.example.com:4443", Host), CancellationToken.None);

    [Fact] void should_not_allow_the_endpoint() => _result.Outcome.ShouldEqual(DirectRedemption.EndpointNotAllowed);
    [Fact] void should_not_let_the_kernel_connect() => _federation.DidNotReceive().Redeem(Arg.Any<string>(), Arg.Any<CancellationToken>());
}
#endif
