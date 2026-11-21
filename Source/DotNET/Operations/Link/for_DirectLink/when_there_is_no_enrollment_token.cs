// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_there_is_no_enrollment_token : a_direct_link
{
    void Establish() => _options = _options with { EnrollmentToken = null };

    async Task Because() => await Pass(Failing("pod:shop/web:CrashLoopBackOff"));

    [Fact] void should_report_that_it_is_not_enrolled() => Status.Connection.ShouldEqual(LinkConnection.NotEnrolled);
    [Fact] void should_not_contact_direct() => _direct.Received.ShouldBeEmpty();
    [Fact] void should_not_queue_anything_it_could_never_deliver() => Status.OutboxDepth.ShouldEqual(0);
}
#endif
