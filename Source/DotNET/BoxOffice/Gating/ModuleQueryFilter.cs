// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Gating;

/// <summary>
/// Refuses a query that belongs to a module the tenant does not have. The query is unauthorized, and the client is told
/// no more than that.
/// </summary>
public class ModuleQueryFilter : IQueryFilter
{
    /// <inheritdoc/>
    public async Task<QueryResult> OnPerform(QueryContext context)
    {
        if (context.ServiceProvider?.GetService<IModuleGate>() is not { } gate)
        {
            return QueryResult.Success(context.CorrelationId);
        }

        return await gate.RefusedModuleOf(context.Name.ToString()) is null
            ? QueryResult.Success(context.CorrelationId)
            : QueryResult.Unauthorized(context.CorrelationId);
    }
}
