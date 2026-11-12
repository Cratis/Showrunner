// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Authoring.Tenancy;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// The rules every audience decision is held to.
/// </summary>
public static class AudienceDecisionRules
{
    /// <summary>
    /// Adds the rules of an audience decision to a validator: the product and the audience are named, the audience name has
    /// the syntax of an audience, the level names exactly the tenant and user it needs, and a tenant is a tenant that exists.
    /// </summary>
    /// <typeparam name="TDecision">The type of command.</typeparam>
    /// <param name="validator">The validator of the command.</param>
    /// <param name="tenants">What the operator side knows of the tenants of a product.</param>
    public static void AddAudienceDecisionRules<TDecision>(this AbstractValidator<TDecision> validator, IKnownTenants tenants)
        where TDecision : IAudienceDecision
    {
        validator.RuleFor(c => (string)c.Product).NotEmpty().WithMessage("Say which product the audience is for.");
        validator.RuleFor(c => c.Level).IsInEnum().WithMessage("Say whether the audience is for a tenant, a user or the system.");
        validator.RuleFor(c => (string)c.Audience)
            .NotEmpty().WithMessage("Name the audience.")
            .Must(name => string.IsNullOrEmpty(name) || AudienceName.IsValidName(name))
            .WithMessage($"An audience name starts with a letter and holds letters, digits and underscores, up to {AudienceName.MaxLength} characters.");

        validator.RuleFor(c => (string)c.TenantId)
            .NotEmpty().WithMessage("Name the tenant.")
            .When(c => c.Level is AudienceLevel.Tenant or AudienceLevel.User);
        validator.RuleFor(c => (string)c.UserId)
            .NotEmpty().WithMessage("Name the user.")
            .When(c => c.Level == AudienceLevel.User);
        validator.RuleFor(c => (string)c.UserId)
            .Empty().WithMessage("Only an audience for a user names a user.")
            .When(c => c.Level is AudienceLevel.Tenant or AudienceLevel.System);
        validator.RuleFor(c => (string)c.TenantId)
            .Empty().WithMessage("An audience for the system names no tenant.")
            .When(c => c.Level == AudienceLevel.System);

        validator.RuleFor(c => c)
            .MustAsync((c, cancellationToken) => tenants.Exists(c.Product, c.TenantId, cancellationToken))
            .WithMessage(c => $"The product '{c.Product.Value}' has no tenant '{c.TenantId.Value}'.")
            .When(c => c.Level is AudienceLevel.Tenant or AudienceLevel.User && !string.IsNullOrWhiteSpace(c.TenantId?.Value) && !string.IsNullOrWhiteSpace(c.Product?.Value));
    }

    /// <summary>
    /// Gets the audience a decision is about, as the operator holds it.
    /// </summary>
    /// <param name="decision">The decision.</param>
    /// <returns>The audience.</returns>
    public static HeldAudience Held(this IAudienceDecision decision) => new(decision.Level, decision.TenantId, decision.UserId, decision.Audience);
}
