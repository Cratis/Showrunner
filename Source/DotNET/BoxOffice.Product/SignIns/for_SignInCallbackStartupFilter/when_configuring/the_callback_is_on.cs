// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCallbackStartupFilter.when_configuring;

public class the_callback_is_on : Specification
{
    int _callback;
    int _other;
    int _wrongMethod;

    async Task Because()
    {
        (_callback, _other, _wrongMethod) = await Pipeline.Run(new() { ReportSignIns = true });
    }

    [Fact] void should_answer_the_callback_on_its_path() => _callback.ShouldEqual(StatusCodes.Status401Unauthorized);
    [Fact] void should_leave_other_paths_to_the_application() => _other.ShouldEqual(StatusCodes.Status404NotFound);
    [Fact] void should_leave_other_methods_to_the_application() => _wrongMethod.ShouldEqual(StatusCodes.Status404NotFound);
}
#endif
