// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Execution;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Suspension.for_TenantAccessFilter.given;

/// <summary>
/// A <see cref="TenantAccessFilter"/> filtering commands of a tenant whose entitlements and signed-in user are substituted.
/// </summary>
public class a_tenant_access_filter : Specification
{
    protected IEntitlements _entitlements;
    protected TenantAccessFilter _filter;
    protected CommandResult _result;
    ServiceProvider _services;

    /// <summary>
    /// Gets whether Box Office is added to the application.
    /// </summary>
    protected virtual bool BoxOfficeIsAdded => true;

    void Establish()
    {
        _entitlements = Substitute.For<IEntitlements>();
        _entitlements.WhyReadOnly().Returns(ReadOnlyReason.None);
        var services = new ServiceCollection();
        if (BoxOfficeIsAdded)
        {
            services.AddSingleton(_entitlements);
        }

        _services = services.BuildServiceProvider();
        _filter = new();
    }

    void Destroy() => _services.Dispose();

    /// <summary>
    /// Runs a command of a type through the filter.
    /// </summary>
    /// <param name="commandType">The type of the command.</param>
    /// <returns>An awaitable task.</returns>
    protected async Task Filter(Type commandType) => _result = await _filter.OnExecution(
        new CommandContext(CorrelationId.New(), commandType, Activator.CreateInstance(commandType)!, [], new CommandContextValues(), null, null, _services));
}
#endif
