// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Contracts.Availability;

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

    /// <summary>
    /// Gets or sets the name of the environment the product reports from, such as Production or Staging. It is told to the
    /// operator with what the product declares and with every decision it received. Not set when the product does not name it.
    /// </summary>
    public EnvironmentName Environment { get; set; } = EnvironmentName.NotSet;

    /// <summary>
    /// Gets or sets the version the product reports its declaration under. When not set, it is the informational version
    /// of the application's entry assembly.
    /// </summary>
    public ModelVersion Version { get; set; } = ModelVersion.NotSet;

    /// <summary>
    /// Gets or sets the audience of the operator the product reports to, when the operator listens for its own. When not
    /// set, what the product reports is addressed to nobody in particular.
    /// </summary>
    public string? OperatorAudience { get; set; }

    /// <summary>
    /// Gets or sets how long to wait before reporting again after reporting failed, for instance while Chronicle is down.
    /// </summary>
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(30);
}
