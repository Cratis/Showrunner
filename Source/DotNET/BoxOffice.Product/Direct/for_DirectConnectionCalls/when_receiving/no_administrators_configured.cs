// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class no_administrators_configured : a_direct_page
{
    int _status;

    async Task Because()
    {
        _options.Direct.Administrators = [];
        _status = (await Open()).Status;
    }

    [Fact] void should_let_nobody_in_not_even_the_first_person() => _status.ShouldEqual(StatusCodes.Status403Forbidden);
}
#endif
