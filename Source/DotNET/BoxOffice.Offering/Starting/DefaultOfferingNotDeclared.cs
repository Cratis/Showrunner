// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Starting;

/// <summary>
/// Thrown when a tenant is started on the default offering of a product that declared none.
/// </summary>
public class DefaultOfferingNotDeclared()
    : InvalidOperationException("The product declared no default offering: call DefaultOffering(...) when declaring the offering in AddBoxOfficeOffering.");
