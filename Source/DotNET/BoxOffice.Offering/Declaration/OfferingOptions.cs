// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// Options for reporting the offering.
/// </summary>
public class OfferingOptions
{
    /// <summary>
    /// Gets or sets how long to wait before trying again when the offering could not be reported, for example
    /// because Chronicle is not up yet. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the event store Box Office reads and records in. Defaults to <see cref="EventStoreName.NotSet"/>, which
    /// means the application's own event store: the one its Cratis or Chronicle setup registers. Set it only to use
    /// another one. <c>BoxOfficeOptions.EventStore</c> sets it too.
    /// </summary>
    public EventStoreName EventStore { get; set; } = EventStoreName.NotSet;

    /// <summary>
    /// Gets or sets the audience of the operator the product reports to. When set, everything the product appends to its
    /// outbox for the operator (the offering, the default offering, seat and compute reports, closed compute days) is
    /// addressed to it, so an operator that subscribes to the product's outbox with <c>ForAudience</c> receives only what
    /// is meant for it. Defaults to <see langword="null" />: nothing is addressed, which is how products behaved before audiences.
    /// <c>BoxOfficeOptions.OperatorAudience</c> sets it too.
    /// </summary>
    public string? OperatorAudience { get; set; }
}
