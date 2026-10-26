// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfForwards.given;
using Microsoft.AspNetCore.Http;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfForwards.when_forwarding;

public class in_another_tenant : a_forwarded_request
{
    void Establish() => _http.Request.Headers[OnBehalfPaths.TenantHeader] = "tenant-two";

    Task Because() => Forwarded();

    [Fact] void should_not_pass_the_request_on() => _passedOn.ShouldBeFalse();
    [Fact] void should_refuse_it() => _http.Response.StatusCode.ShouldEqual(StatusCodes.Status401Unauthorized);
}
#endif
