// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.when_attempting;

public class a_closed_link_that_is_unrecoverable_when_re_enrollment_is_not_allowed : a_direct_connection
{
    DirectLinkState _state;

    async Task Because()
    {
        Redeems();
        Connects(DirectLinkHealth.Healthy);
        await _connection.Attempt(Code, null, CancellationToken.None);
        Connects(DirectLinkHealth.Unrecoverable);
        _state = await _connection.Current(CancellationToken.None);
    }

    [Fact] void should_stay_closed() => _state.Stage.ShouldEqual(DirectLinkStage.Closed);
}
#endif
