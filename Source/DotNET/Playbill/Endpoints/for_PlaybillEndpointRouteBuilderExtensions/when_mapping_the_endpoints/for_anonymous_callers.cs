// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpointRouteBuilderExtensions.when_mapping_the_endpoints;

/// <summary>
/// An application that publishes its release notes can let anyone read them.
/// </summary>
public class for_anonymous_callers : Specification
{
    RouteEndpoint[] _endpoints = [];

    void Because()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddPlaybill(options => options.AllowAnonymous = true);
        var app = builder.Build();
        app.MapPlaybill();
        _endpoints = [.. ((IEndpointRouteBuilder)app).DataSources.SelectMany(_ => _.Endpoints).OfType<RouteEndpoint>()];
    }

    [Fact] void should_allow_anonymous_callers() => _endpoints.All(_ => _.Metadata.GetMetadata<IAllowAnonymous>() is not null).ShouldBeTrue();

    [Fact] void should_not_require_a_signed_in_user() => _endpoints.Any(_ => _.Metadata.GetMetadata<IAuthorizeData>() is not null).ShouldBeFalse();
}
#endif
