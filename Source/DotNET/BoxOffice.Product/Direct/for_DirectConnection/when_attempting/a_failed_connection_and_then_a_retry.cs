// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.when_attempting;

public class a_failed_connection_and_then_a_retry : a_direct_connection
{
    DirectAttemptResult _first;
    DirectLinkStage _stageAfterFirst;
    DirectAttemptResult _second;

    async Task Because()
    {
        Redeems();
        Connects(DirectLinkHealth.Unreachable);
        _first = await _connection.Attempt(Code, null, CancellationToken.None);
        _stageAfterFirst = _record.State.Stage;

        Connects(DirectLinkHealth.Healthy);
        _second = await _connection.Attempt(null, null, CancellationToken.None);
    }

    [Fact] void should_report_the_connection_failed() => _first.ShouldEqual(DirectAttemptResult.ConnectionFailed);
    [Fact] void should_keep_the_page_open_with_the_credentials_stored() => _stageAfterFirst.ShouldEqual(DirectLinkStage.Redeemed);
    [Fact] void should_connect_on_the_retry_without_a_code() => _second.ShouldEqual(DirectAttemptResult.Connected);
    [Fact] void should_close_the_link() => _record.State.Stage.ShouldEqual(DirectLinkStage.Closed);
    [Fact] void should_redeem_only_one_code() => _enrollment.Received(1).Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
