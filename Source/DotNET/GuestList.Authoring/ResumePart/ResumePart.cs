// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Authoring.Tenancy;

namespace Cratis.Showrunner.GuestList.Authoring.ResumePart;

/// <summary>
/// Resumes a part of the product that was suspended for a tenant, a user of a tenant, or the system.
/// </summary>
/// <remarks>
/// A resume ends the suspension of its own level only: resuming a part for a user leaves the tenant's and the system's
/// suspensions of it in place, and a part suspended through a module or feature above it stays suspended until that is
/// resumed. Resuming a part that is not suspended at that level records nothing, so a repeated command is harmless.
/// Which fields name whom is the same as for <c>SuspendPart</c>.
/// </remarks>
/// <param name="Product">The product the part is in.</param>
/// <param name="Level">Whom the part is resumed for.</param>
/// <param name="TenantId">The tenant, for a tenant or a user resume; empty for the system.</param>
/// <param name="UserId">The user, for a user resume; empty otherwise.</param>
/// <param name="Part">The path of the part to resume.</param>
[Command]
[Authorize(Policy = GuestListAuthoring.Policy)]
public record ResumePart(Product Product, SuspensionLevel Level, TenantId TenantId, UserId UserId, PartPath Part) : ISuspensionDecision, ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => new(Product.Value);

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="suspensions">What is suspended for the product, read fresh from the event log.</param>
    /// <returns><see cref="PartResumeDecided"/>, or nothing when the part is not suspended at that level.</returns>
    public IEnumerable<object> Handle(ProductSuspensions? suspensions) =>
        suspensions?.IsSuspended(Level, TenantId, UserId, Part) == true ? [new PartResumeDecided(Level, TenantId, UserId, Part)] : [];
}

/// <summary>
/// Validates <see cref="ResumePart"/>.
/// </summary>
public class ResumePartValidator : CommandValidator<ResumePart>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResumePartValidator"/> class.
    /// </summary>
    /// <param name="tenants">What the operator side knows of the tenants of a product.</param>
    public ResumePartValidator(IKnownTenants tenants) => this.AddSuspensionDecisionRules(tenants);
}
