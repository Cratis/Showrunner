// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// Whether a module offer is paid for or only previewed.
/// </summary>
public enum ModuleAvailability
{
    /// <summary>
    /// The module is paid for.
    /// </summary>
    Paid = 0,

    /// <summary>
    /// The module is open for a preview and not yet sold.
    /// </summary>
    Preview = 1,
}
