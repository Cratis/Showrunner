// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.BoxOffice;

/// <summary>
/// The options of Box Office.
/// </summary>
public class BoxOfficeOptions
{
    /// <summary>
    /// Gets or sets the names of the event stores of the operators whose decisions the product honors. Defaults to
    /// <c>Direct</c>.
    /// </summary>
    public IEnumerable<EventStoreName> OperatorStores { get; set; } = ["Direct"];

    /// <summary>
    /// Gets or sets how often Box Office looks for tenant namespaces it does not serve yet. Defaults to one minute.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMinutes(1);
}
