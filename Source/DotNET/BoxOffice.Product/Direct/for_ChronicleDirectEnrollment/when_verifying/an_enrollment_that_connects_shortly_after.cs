// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_verifying;

public class an_enrollment_that_connects_shortly_after : a_chronicle_direct_enrollment
{
    DirectLinkHealth _result;

    async Task Because()
    {
        _enrollment = new(_client, _logger) { ConnectionPatience = TimeSpan.FromSeconds(30) };
        _federation.GetStatus(Arg.Any<CancellationToken>())
            .Returns(Enrolled(connected: false, exchanged: false), Enrolled(connected: false, exchanged: false), Enrolled(connected: true, exchanged: true));
        _result = await _enrollment.Verify(CancellationToken.None);
    }

    [Fact] void should_be_healthy_once_the_first_exchange_succeeds() => _result.ShouldEqual(DirectLinkHealth.Healthy);
}
#endif
