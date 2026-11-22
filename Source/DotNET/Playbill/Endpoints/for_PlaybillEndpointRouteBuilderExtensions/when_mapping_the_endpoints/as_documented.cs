// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpointRouteBuilderExtensions.when_mapping_the_endpoints;

/// <summary>
/// The four endpoints are mapped under the versioned path and, by default, for signed-in users only.
/// </summary>
public class as_documented : Specification
{
    RouteEndpoint[] _endpoints = [];
    Exception _withoutPlaybill;

    void Because()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddPlaybill();
        var app = builder.Build();
        app.MapPlaybill();
        _endpoints = [.. ((IEndpointRouteBuilder)app).DataSources.SelectMany(_ => _.Endpoints).OfType<RouteEndpoint>()];
        _withoutPlaybill = Record.Exception(WebApplication.CreateBuilder().Build().MapPlaybill);
    }

    [Fact] void should_map_the_four_endpoints() => _endpoints.Select(_ => _.RoutePattern.RawText).ShouldContainOnly(
        "/.cratis/playbill/v1/releases",
        "/.cratis/playbill/v1/releases/{version}",
        "/.cratis/playbill/v1/releases/{version}/{**asset}",
        "/.cratis/playbill/v1/current");

    [Fact] void should_require_a_signed_in_user() => _endpoints.All(_ => _.Metadata.GetMetadata<IAuthorizeData>() is not null).ShouldBeTrue();

    [Fact] void should_not_allow_anonymous_callers() => _endpoints.Any(_ => _.Metadata.GetMetadata<IAllowAnonymous>() is not null).ShouldBeFalse();

    [Fact] void should_refuse_a_host_that_did_not_add_playbill() => _withoutPlaybill.ShouldBeOfExactType<InvalidOperationException>();
}
#endif
