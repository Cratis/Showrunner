// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// How far the operator's decisions have reached a product, for each environment it reports from: the newest decision the
/// product told the operator it recorded.
/// </summary>
/// <remarks>
/// <para>
/// It is a high-water mark and not a ledger of decisions. A decision is waiting when it was taken after what
/// <see cref="EnvironmentDelivery.ReceivedThrough"/> says, which <see cref="EnvironmentDelivery.IsWaiting"/> answers. Chronicle
/// delivers the decisions of one product in order, so a decision the product recorded is never behind one it did not.
/// </para>
/// <para>
/// It is built from <see cref="AudienceDecisionReceived"/>, which the product publishes once per decision it recorded. It
/// never says the product is connected: when nothing has been received, the decisions are waiting, which is all that is known.
/// </para>
/// </remarks>
/// <param name="Product">The product.</param>
/// <param name="Environments">What reached each environment, in the order of the environments' names.</param>
[ReadModel]
public record AudienceDelivery(Product Product, IEnumerable<EnvironmentDelivery> Environments)
{
    /// <summary>
    /// Observes how far the operator's decisions have reached a product.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="readModels">The <see cref="IReadModels"/>.</param>
    /// <returns>An observable of the delivery; it holds one item once the product has received a decision.</returns>
    [Authorize(Policy = GuestListAuthoring.Policy)]
    public static System.Reactive.Subjects.ISubject<IEnumerable<AudienceDelivery>> ForProduct(Product product, IReadModels readModels) =>
        readModels.Observe<AudienceDelivery>(delivery => delivery.Product == product);

    /// <summary>
    /// Takes a receipt into the high-water mark of its environment.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The event context.</param>
    /// <returns>The next state.</returns>
    public static AudienceDelivery Reduce(AudienceDecisionReceived @event, AudienceDelivery? current, EventContext context)
    {
        var state = current ?? new(new Product(context.EventSourceId.Value), []);
        var previous = state.Environments.FirstOrDefault(environment => environment.Environment == @event.Environment);
        var delivery = new EnvironmentDelivery(
            @event.Environment,
            previous is not null && previous.ReceivedThrough > @event.DecidedAt ? previous.ReceivedThrough : @event.DecidedAt,
            previous is not null && previous.LastReceivedAt > context.Occurred ? previous.LastReceivedAt : context.Occurred);

        return state with
        {
            Environments = [.. state.Environments.Where(environment => environment.Environment != @event.Environment).Append(delivery).OrderBy(environment => environment.Environment.Value, StringComparer.Ordinal)]
        };
    }
}
