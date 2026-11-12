// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.GuestList;

/// <summary>
/// Represents how a product configures GuestList.
/// </summary>
public class GuestListOptions
{
    /// <summary>
    /// Gets or sets the product this application is, as the operator's decisions name it. An operator store serves several
    /// products and a decision is for one of them; the product records only its own. Required.
    /// </summary>
    public Product Product { get; set; } = Product.NotSet;

    /// <summary>
    /// Gets or sets the names of the event stores whose outbox the decisions come from, which is where the authoring slices
    /// of <c>Cratis.Showrunner.GuestList.Authoring</c> run. The contracts name no store, so nothing routes them on its own:
    /// the subscription is explicit. Defaults to <c>Direct</c>, which is only a name; Direct is not called.
    /// </summary>
    public IEnumerable<EventStoreName> OperatorStores { get; set; } = ["Direct"];

    /// <summary>
    /// Gets or sets whether GuestList subscribes to the outbox of each operator store for this product's routing audience only
    /// (the value of <see cref="Product"/>), so it receives the decisions made for this product and nothing meant for another
    /// product on the same operator store. The routing audience is Chronicle's own label on a subscription, not a field of
    /// the events. Turn it off only when the operator side does not address decisions to products
    /// (<c>GuestListAuthoringOptions.AddressToProducts</c>), or nothing arrives. Defaults to <see langword="true"/>.
    /// </summary>
    public bool SubscribeForProduct { get; set; } = true;

    /// <summary>
    /// Gets or sets the audiences the application declares, usually the audiences of its model. A grant of any other
    /// audience is kept and carried as recorded, but it never matches a gate and is reported by the undeclared grants query.
    /// <see langword="null"/>, the default, means the application declares nothing yet, so nothing is reported as undeclared.
    /// </summary>
    public IEnumerable<AudienceName>? DeclaredAudiences { get; set; }

    /// <summary>
    /// Gets or sets how often GuestList makes sure the product is subscribed to the operator stores and observes their
    /// inbox. Everything it acts on is durable, so a round that fails is retried at the next one. Defaults to one minute.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMinutes(1);
}
