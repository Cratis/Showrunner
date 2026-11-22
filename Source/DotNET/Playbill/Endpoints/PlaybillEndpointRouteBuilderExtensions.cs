// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Playbill;
using Cratis.Showrunner.Playbill.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for mapping the endpoints that serve an application's release history.
/// </summary>
public static class PlaybillEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the release history under <c>/.cratis/playbill/v1</c>: <c>GET releases</c>, <c>GET releases/{version}</c>,
    /// <c>GET releases/{version}/{asset}</c> and <c>GET current</c>.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/>.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for continuation.</returns>
    /// <exception cref="InvalidOperationException">The host did not call <c>AddPlaybill</c>.</exception>
    /// <remarks>
    /// The endpoints require a signed-in user, because release notes can describe behavior that is not for everyone. Set
    /// <see cref="PlaybillOptions.AllowAnonymous"/> to serve them to anyone.
    /// </remarks>
    public static IEndpointRouteBuilder MapPlaybill(this IEndpointRouteBuilder endpoints)
    {
        var options = endpoints.ServiceProvider.GetService<IOptions<PlaybillOptions>>();
        if (options is null || endpoints.ServiceProvider.GetService<IReleaseNotes>() is null)
        {
            throw new InvalidOperationException("MapPlaybill needs AddPlaybill; call it on the service collection first.");
        }

        var group = endpoints.MapGroup(PlaybillPaths.Group).ExcludeFromDescription();
        if (options.Value.AllowAnonymous)
        {
            group.AllowAnonymous();
        }
        else
        {
            group.RequireAuthorization();
        }

        group.MapGet(PlaybillPaths.Releases, (IReleaseNotes notes, string? since) => PlaybillEndpoints.List(notes, since));
        group.MapGet(PlaybillPaths.Release, (IReleaseNotes notes, string version, HttpContext http) => PlaybillEndpoints.Get(notes, version, http));
        group.MapGet(PlaybillPaths.Asset, (IReleaseNotes notes, string version, string asset) => PlaybillEndpoints.GetAsset(notes, version, asset));
        group.MapGet(PlaybillPaths.Current, (IReleaseNotes notes) => PlaybillEndpoints.GetCurrent(notes));
        return endpoints;
    }
}
