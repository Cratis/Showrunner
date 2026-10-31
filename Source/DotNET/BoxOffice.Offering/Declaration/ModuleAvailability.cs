// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// How a module can be had.
/// </summary>
public enum ModuleAvailability
{
    /// <summary>
    /// Every tenant has the module; it is part of the product.
    /// </summary>
    Included = 0,

    /// <summary>
    /// A tenant has the module when it is granted or bought.
    /// </summary>
    Paid = 1,

    /// <summary>
    /// A tenant can open the module as a preview before it is released.
    /// </summary>
    Preview = 2,

    /// <summary>
    /// The module is announced but cannot be had yet.
    /// </summary>
    Coming = 3
}
