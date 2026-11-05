// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.Reactors;
using Cratis.Showrunner.BoxOffice.Receiving;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning;

/// <summary>
/// Listens to what each operator tells the product about a tenant's life, on the inbox Box Office keeps for the operator.
/// </summary>
/// <param name="scopes">The <see cref="IServiceScopeFactory"/> that gives each delivery its own services.</param>
/// <param name="options">The <see cref="BoxOfficeOptions"/>, which name the operators.</param>
public class ProvisioningSubscriptions(IServiceScopeFactory scopes, IOptions<BoxOfficeOptions> options)
{
    readonly HashSet<EventStoreName> _registered = [];
    readonly Lock _lock = new();

    /// <summary>
    /// Gets the identifier of the reactor that listens to an operator.
    /// </summary>
    /// <param name="operatorStore">The name of the operator's event store.</param>
    /// <returns>The <see cref="ReactorId"/>.</returns>
    public static ReactorId ReactorFor(EventStoreName operatorStore) => new($"Cratis.Showrunner.BoxOffice.Product.Provisioning.TenantRegistrations.{operatorStore.Value}");

    /// <summary>
    /// Registers the reactors, once per operator.
    /// </summary>
    /// <param name="eventStore">The product's <see cref="IEventStore"/> whose inbox the operator's events arrive in.</param>
    /// <returns>A <see cref="Task"/> for continuation.</returns>
    public async Task Register(IEventStore eventStore)
    {
        foreach (var operatorStore in options.Value.OperatorStores)
        {
            lock (_lock)
            {
                if (_registered.Contains(operatorStore))
                {
                    continue;
                }
            }

            await eventStore.Reactors.Register(ReactorFor(operatorStore), reactor => reactor
                .OnEventSequence(OperatorSubscriptions.InboxFor(operatorStore))
                .NotReplayable()
                .On<TenantRegistered>((registered, context) => Deliver(handler => handler.Registered(registered, context)))
                .On<TenantClosing>((closing, _) => Deliver(handler => handler.Closing(closing))));

            lock (_lock)
            {
                _registered.Add(operatorStore);
            }
        }
    }

    async Task Deliver(Func<TenantRegistrations, Task> deliver)
    {
        await using var scope = scopes.CreateAsyncScope();
        await deliver(scope.ServiceProvider.GetRequiredService<TenantRegistrations>());
    }
}
