// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_ChronicleDirectEnrollment.when_verifying;

public class an_enrollment_that_has_expired : a_chronicle_direct_enrollment
{
    DirectLinkHealth _result;

    async Task Because()
    {
        Status(Enrolled(expired: true));
        _result = await _enrollment.Verify(CancellationToken.None);
    }

    [Fact] void should_be_unrecoverable() => _result.ShouldEqual(DirectLinkHealth.Unrecoverable);
}
#endif
