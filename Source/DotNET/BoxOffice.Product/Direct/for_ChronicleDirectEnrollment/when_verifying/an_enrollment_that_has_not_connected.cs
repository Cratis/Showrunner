// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_verifying;

public class an_enrollment_that_has_not_connected : a_chronicle_direct_enrollment
{
    DirectLinkHealth _result;

    async Task Because()
    {
        Status(Enrolled(connected: false, exchanged: false, lastError: "the authority refused the session"));
        _result = await _enrollment.Verify(CancellationToken.None);
    }

    [Fact] void should_be_unreachable_because_no_exchange_has_succeeded() => _result.ShouldEqual(DirectLinkHealth.Unreachable);
    [Fact] void should_log_that_it_is_not_connected() => Logged().Any(_ => _ == "NotConnected").ShouldBeTrue();
}
#endif
