// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Availability;

/// <summary>
/// What the tenant may do with one module, as an availability gate reads it:
/// <c>$context.entitlements.modules.&lt;module&gt;.level</c> and <c>.usable</c>.
/// </summary>
/// <param name="Level">The level: <c>None</c>, <c>Preview</c>, <c>Trial</c> or <c>Full</c>.</param>
/// <param name="Usable">Whether the tenant may use the module, which it may on a trial or in full. A preview is not usable.</param>
public record ModuleEntitlementValue(string Level, bool Usable);
