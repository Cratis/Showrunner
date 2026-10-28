// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Cratis.Arc.Tenancy;
using Cratis.Showrunner.Usher.Identity;

namespace Cratis.Showrunner.Usher.Reporting.Capture;

/// <summary>
/// Represents the <see cref="ICurrentErrorOwner"/> taken from the signed-in reporter and Arc's current tenant.
/// </summary>
/// <param name="reporter">The <see cref="ISignedInReporter"/>.</param>
/// <param name="tenants">The <see cref="ITenantIdAccessor"/> holding the current tenant.</param>
public class CurrentErrorOwner(ISignedInReporter reporter, ITenantIdAccessor tenants) : ICurrentErrorOwner
{
    /// <inheritdoc/>
    public bool TryGet([NotNullWhen(true)] out ErrorOwner? owner)
    {
        owner = reporter.IsSignedIn ? new ErrorOwner(tenants.Current.Value, reporter.Identity) : null;
        return owner is not null;
    }
}
