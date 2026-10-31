// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.given;

/// <summary>
/// A command of the product's maintainers over a tenant's compute, for a product that declares an <c>agent-runs</c> meter with a
/// concurrency limit and a <c>tokens</c> meter without one.
/// </summary>
/// <typeparam name="TCommand">The type of command.</typeparam>
public class a_compute_command<TCommand> : a_seat_command<TCommand>
    where TCommand : class
{
    /// <summary>
    /// The meter with a concurrency limit.
    /// </summary>
    protected static readonly MeterId AgentRuns = new("agent-runs");

    /// <summary>
    /// The meter without a concurrency limit.
    /// </summary>
    protected static readonly MeterId Tokens = new("tokens");

    void Establish() => _scenario.Services.AddSingleton(new ProductOfferingBuilder()
        .Product(ProductId.New())
        .Meter(AgentRuns, "Agent runs", "runs", hasConcurrencyLimit: true)
        .Meter(Tokens, "Tokens", "tokens")
        .Build());
}
#endif
