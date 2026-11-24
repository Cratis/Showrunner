// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Authoring.Tenancy;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// The rules every part suspension decision is held to.
/// </summary>
public static class SuspensionDecisionRules
{
    /// <summary>
    /// Adds the rules of a suspension decision to a validator: the product and the part are named, the part has the syntax
    /// of a part path, the level names exactly the tenant and user it needs, and a tenant is a tenant that exists.
    /// </summary>
    /// <typeparam name="TDecision">The type of command.</typeparam>
    /// <param name="validator">The validator of the command.</param>
    /// <param name="tenants">What the operator side knows of the tenants of a product.</param>
    public static void AddSuspensionDecisionRules<TDecision>(this AbstractValidator<TDecision> validator, IKnownTenants tenants)
        where TDecision : ISuspensionDecision
    {
        validator.RuleFor(c => (string)c.Product).NotEmpty().WithMessage("Say which product the part is for.");
        validator.RuleFor(c => c.Level).IsInEnum().WithMessage("Say whether the part is for a tenant, a user or the system.");
        validator.RuleFor(c => (string)c.Part)
            .NotEmpty().WithMessage("Name the part.")
            .Must(path => string.IsNullOrEmpty(path) || PartPath.IsValidPath(path))
            .WithMessage("A part is a path of names joined with '/', for example Fleet/DataQuality.");

        validator.RuleFor(c => (string)c.TenantId)
            .NotEmpty().WithMessage("Name the tenant.")
            .When(c => c.Level is SuspensionLevel.Tenant or SuspensionLevel.User);
        validator.RuleFor(c => (string)c.UserId)
            .NotEmpty().WithMessage("Name the user.")
            .When(c => c.Level == SuspensionLevel.User);
        validator.RuleFor(c => (string)c.UserId)
            .Empty().WithMessage("Only a suspension for a user names a user.")
            .When(c => c.Level is SuspensionLevel.Tenant or SuspensionLevel.System);
        validator.RuleFor(c => (string)c.TenantId)
            .Empty().WithMessage("A suspension for the system names no tenant.")
            .When(c => c.Level == SuspensionLevel.System);
        validator.RuleFor(c => c)
            .MustAsync((c, cancellationToken) => tenants.Exists(c.Product, c.TenantId, cancellationToken))
            .WithMessage(c => $"The product '{c.Product.Value}' has no tenant '{c.TenantId.Value}'.")
            .When(c => c.Level is SuspensionLevel.Tenant or SuspensionLevel.User && !string.IsNullOrWhiteSpace(c.TenantId?.Value) && !string.IsNullOrWhiteSpace(c.Product?.Value));
    }
}
