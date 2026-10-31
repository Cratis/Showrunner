// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// A module offer was given the module it sells and how it is sold.
/// </summary>
/// <param name="Module">The module.</param>
/// <param name="Charging">How it is charged.</param>
/// <param name="Trial">How many days it can be tried before it is paid for.</param>
/// <param name="Availability">Whether it is paid for or only previewed.</param>
[EventType]
public record ModuleOfferSpecified(ModuleId Module, ModuleCharging Charging, TrialLength Trial, ModuleAvailability Availability);
