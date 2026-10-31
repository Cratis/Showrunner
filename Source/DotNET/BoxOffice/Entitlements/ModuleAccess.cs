// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// What a tenant may do with a module.
/// </summary>
public enum ModuleAccess
{
    /// <summary>
    /// The tenant has nothing to do with the module.
    /// </summary>
    None = 0,

    /// <summary>
    /// The tenant may look at the module but not use it.
    /// </summary>
    Preview = 1,

    /// <summary>
    /// The tenant may use the module for a trial period.
    /// </summary>
    Trial = 2,

    /// <summary>
    /// The tenant has the module.
    /// </summary>
    Full = 3
}
