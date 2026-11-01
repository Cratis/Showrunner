// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.Collecting.Transport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Notes.Collecting.for_NoteCollectingEndpointRouteBuilderExtensions;

public class when_mapping_the_endpoints : Specification
{
    RouteEndpoint[] _endpoints = [];
    Exception _withoutAResolver;

    void Because()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Substitute.For<IChronicleClient>());
        builder.Services.AddNoteCollecting(_ => { });
        builder.Services.Configure<ChronicleClientOptions>(_ => _.EventStore = "Studio");
        builder.Services.AddSingleton(Substitute.For<IResolveNotesPublications>());
        var app = builder.Build();
        ((IEndpointRouteBuilder)app).MapNoteCollecting();
        _endpoints = [.. ((IEndpointRouteBuilder)app).DataSources.SelectMany(_ => _.Endpoints).OfType<RouteEndpoint>()];

        var bare = WebApplication.CreateBuilder().Build();
        _withoutAResolver = Record.Exception(() => ((IEndpointRouteBuilder)bare).MapNoteCollecting());
    }

    [Fact] void should_map_the_three_endpoints() => _endpoints.Select(_ => _.RoutePattern.RawText).ShouldContainOnly(
        "/.cratis/notes/v1/publications/{publicationId}/notes",
        "/.cratis/notes/v1/publications/{publicationId}/notes/position",
        "/.cratis/notes/v1/publications/{publicationId}/responses");
    [Fact] void should_not_ask_for_the_hosts_user_authentication() => _endpoints.All(_ => _.Metadata.GetMetadata<IAllowAnonymous>() is not null).ShouldBeTrue();
    [Fact] void should_refuse_a_host_without_a_resolver() => _withoutAResolver.ShouldBeOfExactType<InvalidOperationException>();
}
#endif
