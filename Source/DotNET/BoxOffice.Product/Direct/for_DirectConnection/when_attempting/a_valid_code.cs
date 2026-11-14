// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.when_attempting;

public class a_valid_code : a_direct_connection
{
    DirectAttemptResult _result;

    async Task Because()
    {
        Redeems();
        Connects(DirectLinkHealth.Healthy);
        _result = await _connection.Attempt(Code, null, CancellationToken.None);
    }

    [Fact] void should_connect() => _result.ShouldEqual(DirectAttemptResult.Connected);
    [Fact] void should_close_the_link_for_good() => _record.State.Stage.ShouldEqual(DirectLinkStage.Closed);
    [Fact] void should_remember_the_endpoint_of_the_code() => _record.State.Endpoint.ShouldEqual(Endpoint);
    [Fact] void should_redeem_the_code_once() => _enrollment.Received(1).Redeem(Arg.Is<DirectEnrollmentRequest>(request => request.Code == Code), Arg.Any<CancellationToken>());
}
#endif
