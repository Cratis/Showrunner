// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Usher.Reporting.Capture;

/// <summary>
/// Represents whose a captured error is: the signed-in user in a tenant. Only the same user in the same tenant can use
/// the captured error in a report.
/// </summary>
/// <param name="Tenant">The <see cref="TenantId"/> the error happened in.</param>
/// <param name="Subject">The <see cref="IdentityProviderSubject"/> of the user the error happened for.</param>
public record ErrorOwner(TenantId Tenant, IdentityProviderSubject Subject);
