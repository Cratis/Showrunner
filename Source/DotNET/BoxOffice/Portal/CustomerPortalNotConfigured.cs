// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Portal;

/// <summary>
/// Thrown when a link to the customer portal is asked for, and the product has no customer portal.
/// </summary>
public class CustomerPortalNotConfigured()
    : Exception("The product has no customer portal. Set BoxOfficeOptions.CustomerPortalUrl and BoxOfficeOptions.CustomerPortalSigningKey in AddBoxOffice.");
