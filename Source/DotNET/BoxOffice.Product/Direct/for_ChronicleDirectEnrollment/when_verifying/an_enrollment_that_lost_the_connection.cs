// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_verifying;

public class an_enrollment_that_lost_the_connection : a_chronicle_direct_enrollment
{
    DirectLinkHealth _result;

    async Task Because()
    {
        _enrollment = new(_client, _logger) { ConnectionPatience = TimeSpan.FromMinutes(5) };
        Status(Enrolled(connected: false, exchanged: true, lastError: "the session was lost"));
        _result = await _enrollment.Verify(CancellationToken.None);
    }

    [Fact] void should_be_unreachable_without_waiting() => _result.ShouldEqual(DirectLinkHealth.Unreachable);
}
#endif
