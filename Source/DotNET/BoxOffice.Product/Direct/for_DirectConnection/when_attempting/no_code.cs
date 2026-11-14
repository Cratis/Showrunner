// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.when_attempting;

public class no_code : a_direct_connection
{
    DirectAttemptResult _result;

    async Task Because() => _result = await _connection.Attempt("   ", null, CancellationToken.None);

    [Fact] void should_ask_for_a_code() => _result.ShouldEqual(DirectAttemptResult.NoCode);
    [Fact] void should_not_redeem_anything() => _enrollment.DidNotReceive().Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
