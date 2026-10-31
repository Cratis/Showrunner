// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// The exception that is thrown when work is admitted against a meter the product did not declare.
/// </summary>
/// <param name="meter">The meter.</param>
public class MeterNotDeclared(MeterId meter)
    : Exception($"The product did not declare a meter '{meter.Value}', so work can not be admitted against it. Declare it with Meter() in AddBoxOfficeOffering.");
