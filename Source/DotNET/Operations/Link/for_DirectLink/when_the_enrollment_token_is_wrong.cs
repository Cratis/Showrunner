// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_the_enrollment_token_is_wrong : a_direct_link
{
    void Establish() => _options = _options with { EnrollmentToken = "not-the-token" };

    async Task Because() => await Pass();

    [Fact] void should_report_that_direct_did_not_accept_it() => Status.Connection.ShouldEqual(LinkConnection.Unauthorized);
    [Fact] void should_keep_no_credential() => _secrets.Values.ShouldBeEmpty();
    [Fact] void should_not_put_the_token_in_the_status() => Status.Problems.Any(problem => problem.Contains("not-the-token")).ShouldBeFalse();
}
#endif
