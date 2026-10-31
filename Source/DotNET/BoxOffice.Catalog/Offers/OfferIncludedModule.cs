// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers;

/// <summary>
/// An offer was made to include a module.
/// </summary>
/// <param name="Module">The module.</param>
[EventType]
public record OfferIncludedModule(ModuleId Module);
