// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.AspNetCore.Http;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCallbackStartupFilter.when_configuring;

public class the_callback_is_off : Specification
{
    int _callback;

    async Task Because()
    {
        (_callback, _, _) = await Pipeline.Run(new() { ReportSignIns = false });
    }

    [Fact] void should_leave_the_callback_path_to_the_application() => _callback.ShouldEqual(StatusCodes.Status404NotFound);
}
#endif
