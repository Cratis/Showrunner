// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring;

/// <summary>
/// Represents how an authoring application configures what it publishes to products.
/// </summary>
public class GuestListAuthoringOptions
{
    /// <summary>
    /// Gets or sets whether each decision is addressed to the product it is for: the outbox event carries the routing audience
    /// of the product (the value of its <c>Product</c>), so a product that subscribes for its own audience
    /// (<c>GuestListOptions.SubscribeForProduct</c>) receives only its own decisions. The routing audience is Chronicle's own
    /// label on an event, not a field of the contract events. Defaults to <see langword="true"/>.
    /// </summary>
    public bool AddressToProducts { get; set; } = true;

    /// <summary>
    /// Gets or sets, for each product, the audiences its application declares: what the operator knows of a product's model.
    /// It is what the undeclared grants report compares the grants of the product with. A product that is not in the
    /// dictionary has declared nothing the operator knows of, so nothing is reported as undeclared for it.
    /// A declaration the product reported itself (<c>AvailabilityDeclared</c>) takes precedence over what is configured here,
    /// which stays as the fallback for a product that has not reported.
    /// </summary>
    public IDictionary<Product, IEnumerable<AudienceName>> DeclaredAudiences { get; set; } = new Dictionary<Product, IEnumerable<AudienceName>>();

    /// <summary>
    /// Gets or sets the environment whose declaration counts when a product reports from more than one. Defaults to Production.
    /// </summary>
    public EnvironmentName PrimaryEnvironment { get; set; } = "Production";
}
