// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class a_call_without_a_principal : a_direct_page
{
    int _status;

    async Task Because() => _status = (await Send(Request(HttpMethods.Get, null))).Status;

    [Fact] void should_be_unauthorized() => _status.ShouldEqual(StatusCodes.Status401Unauthorized);
}
#endif
