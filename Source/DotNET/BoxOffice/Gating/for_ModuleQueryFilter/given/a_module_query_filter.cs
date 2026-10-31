// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Queries;
using Cratis.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleQueryFilter.given;

/// <summary>
/// A <see cref="ModuleQueryFilter"/> filtering queries of a tenant whose module gate is substituted.
/// </summary>
public class a_module_query_filter : Specification
{
    protected IModuleGate _gate;
    protected ModuleQueryFilter _filter;
    protected QueryResult _result;
    ServiceProvider _services;

    /// <summary>
    /// Gets whether Box Office is added to the application.
    /// </summary>
    protected virtual bool BoxOfficeIsAdded => true;

    void Establish()
    {
        _gate = Substitute.For<IModuleGate>();
        var services = new ServiceCollection();
        if (BoxOfficeIsAdded)
        {
            services.AddSingleton(_gate);
        }

        _services = services.BuildServiceProvider();
        _filter = new();
    }

    void Destroy() => _services.Dispose();

    /// <summary>
    /// Runs a query through the filter.
    /// </summary>
    /// <param name="name">The fully qualified name of the query.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task Filter(string name) => _result = await _filter.OnPerform(
        new QueryContext(new FullyQualifiedQueryName(name), CorrelationId.New(), Paging.NotPaged, Sorting.None, QueryArguments.Empty, [], _services, CancellationToken.None));
}
#endif
