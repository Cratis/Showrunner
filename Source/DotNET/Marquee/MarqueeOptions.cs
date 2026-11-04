// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.Marquee;

/// <summary>
/// Represents how a product configures Marquee.
/// </summary>
public class MarqueeOptions
{
    /// <summary>
    /// Gets or sets the product this application is, as the announcements name it. An authoring store serves several
    /// products and an announcement belongs to one of them; the product shows only its own. Required.
    /// </summary>
    public Product Product { get; set; } = Product.NotSet;

    /// <summary>
    /// Gets or sets the names of the event stores whose outbox the announcements come from, which is where the
    /// authoring slices of <c>Cratis.Showrunner.Marquee.Authoring</c> run. The contracts name no store, so nothing routes
    /// them on its own: the subscription is explicit. Defaults to <c>Direct</c>.
    /// </summary>
    public IEnumerable<EventStoreName> AuthoringStores { get; set; } = ["Direct"];

    /// <summary>
    /// Gets or sets whether Marquee subscribes to the outbox of each authoring store for this product's audience only (the
    /// value of <see cref="Product"/>), so it receives the announcements authored for this product and nothing meant for
    /// another product on the same authoring store. Turn it on only when the authoring side addresses announcements to
    /// products (<c>MarqueeAuthoringOptions.AddressToProducts</c>), or nothing arrives. Defaults to <see langword="false" />:
    /// the subscription is unfiltered, as before audiences.
    /// </summary>
    public bool SubscribeForProduct { get; set; }

    /// <summary>
    /// Gets or sets how often Marquee makes sure the product is subscribed to the authoring stores and observes their
    /// inbox. Everything it acts on is durable, so a round that fails is retried at the next one. Defaults to one minute.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMinutes(1);
}
