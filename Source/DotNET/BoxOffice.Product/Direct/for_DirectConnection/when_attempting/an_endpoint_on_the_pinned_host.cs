// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.when_attempting;

public class an_endpoint_on_the_pinned_host : a_direct_connection
{
    async Task Because()
    {
        _options.Direct.ExpectedEndpointHost = "chronicle.example.com";
        Redeems();
        Connects(DirectLinkHealth.Healthy);
        await _connection.Attempt(Code, Endpoint, CancellationToken.None);
    }

    [Fact] void should_redeem_the_code_for_that_endpoint_and_tell_the_port_the_pinned_host() =>
        _enrollment.Received(1).Redeem(Arg.Is<DirectEnrollmentRequest>(request => request.Endpoint == Endpoint && request.ExpectedHost == "chronicle.example.com"), Arg.Any<CancellationToken>());
}
#endif
