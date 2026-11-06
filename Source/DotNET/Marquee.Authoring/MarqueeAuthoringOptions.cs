// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring;

/// <summary>
/// Represents how an authoring application configures what it publishes to products.
/// </summary>
public class MarqueeAuthoringOptions
{
    /// <summary>
    /// Gets or sets whether each announcement is addressed to the product it is for: the outbox event carries the audience
    /// of the product (the value of its <c>Product</c>), so a product that subscribes with
    /// <c>MarqueeOptions.SubscribeForProduct</c> receives only its own announcements. Defaults to <see langword="false" />:
    /// nothing is addressed, as before audiences.
    /// </summary>
    public bool AddressToProducts { get; set; }
}
