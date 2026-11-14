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
    /// the syntax of an audience, the level names exactly the tenant, user and system it needs, and a tenant is a tenant that exists.
    /// </summary>
    /// <typeparam name="TDecision">The type of command.</typeparam>
    /// <param name="validator">The validator of the command.</param>
    /// <param name="tenants">What the operator side knows of the tenants of a product.</param>
    public static void AddAudienceDecisionRules<TDecision>(this AbstractValidator<TDecision> validator, IKnownTenants tenants)
        where TDecision : IAudienceDecision
    {
        validator.RuleFor(c => (string)c.Product).NotEmpty().WithMessage("Say which product the audience is for.");
        validator.RuleFor(c => c.Level).IsInEnum().WithMessage("Say whether the audience is for a tenant, a user, the system or one named system.");
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
            .When(c => c.Level is AudienceLevel.Tenant or AudienceLevel.System or AudienceLevel.NamedSystem);
        validator.RuleFor(c => (string)c.TenantId)
            .Empty().WithMessage("An audience for the system names no tenant.")
            .When(c => c.Level is AudienceLevel.System or AudienceLevel.NamedSystem);

        validator.RuleFor(c => ClientText(c.ClientId))
            .NotEmpty().WithMessage("Name the client id of the system.")
            .MaximumLength(SystemClientId.MaxLength).WithMessage($"A client id is at most {SystemClientId.MaxLength} characters.")
            .When(c => c.Level == AudienceLevel.NamedSystem);
        validator.RuleFor(c => ClientText(c.ClientId))
            .Empty().WithMessage("Only an audience for a named system names a client id.")
            .When(c => c.Level != AudienceLevel.NamedSystem);
        validator.RuleFor(c => c.Kind)
            .IsInEnum().NotEqual(SystemKind.None).WithMessage("Say what kind of system it is: MCP, API or other.")
            .When(c => c.Level == AudienceLevel.NamedSystem);
        validator.RuleFor(c => c.Kind)
            .Equal(SystemKind.None).WithMessage("Only an audience for a named system names a kind.")
            .When(c => c.Level != AudienceLevel.NamedSystem);

        validator.RuleFor(c => c)
            .MustAsync((c, cancellationToken) => tenants.Exists(c.Product, c.TenantId, cancellationToken))
            .WithMessage(c => $"The product '{c.Product.Value}' has no tenant '{c.TenantId.Value}'.")
            .When(c => c.Level is AudienceLevel.Tenant or AudienceLevel.User && !string.IsNullOrWhiteSpace(c.TenantId?.Value) && !string.IsNullOrWhiteSpace(c.Product?.Value));
    }

    static string ClientText(SystemClientId? client) => client?.Value ?? string.Empty;

    /// <summary>
    /// Gets the audience a decision is about, as the operator holds it.
    /// </summary>
    /// <param name="decision">The decision.</param>
    /// <returns>The audience.</returns>
    public static HeldAudience Held(this IAudienceDecision decision) => new(decision.Level, decision.TenantId, decision.UserId, decision.Audience, decision.ClientId ?? SystemClientId.NotSet, decision.Kind);
}
