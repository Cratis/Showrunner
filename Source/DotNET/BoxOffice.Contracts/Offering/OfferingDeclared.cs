// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offering;

/// <summary>
/// Published by the product, once per change, to say what it sells: its modules, its meters and how it licenses
/// seats. Direct builds its entitlement and offer screens from it.
/// </summary>
/// <param name="Modules">The modules the product has.</param>
/// <param name="Meters">The meters the product has.</param>
/// <param name="Seats">How the product licenses seats.</param>
[EventType]
[Public]
public record OfferingDeclared(IEnumerable<ModuleDeclaration> Modules, IEnumerable<MeterDeclaration> Meters, SeatLicensing Seats);
