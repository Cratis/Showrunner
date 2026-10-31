// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Offering;

/// <summary>
/// A module a product declares.
/// </summary>
/// <param name="Id">The module's id.</param>
/// <param name="Name">The module's name.</param>
public record ModuleDeclaration(
    ModuleId Id,
    ModuleName Name);
