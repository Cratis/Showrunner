// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// The onboarding service of an application: exactly one Ante. Declaring it turns Ante on; leave <see cref="PerformanceArgs.Onboarding"/> out and none runs.
/// </summary>
/// <remarks>
/// Onboarding needs <see cref="PerformanceArgs.AuthProxy"/>, <see cref="PerformanceArgs.Ingress"/> and the key <c>ante.mongodb.server</c> in the Cratis environment.
/// </remarks>
public sealed class OnboardingArgs
{
    /// <summary>
    /// Gets the Ante image, or null for the operator's default.
    /// </summary>
    public string? Image { get; init; }

    /// <summary>
    /// Gets the port Ante listens on, or null for the operator's default.
    /// </summary>
    public int? Port { get; init; }

    /// <summary>
    /// Gets Ante's own event store, or null for <c>ante-NAME</c>.
    /// </summary>
    public string? EventStore { get; init; }

    /// <summary>
    /// Gets the Chronicle namespace Ante runs in, or null for the operator's default.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets what self-service sign-up does, or null for the operator's default.
    /// </summary>
    public SignUpMode? SignUp { get; init; }

    /// <summary>
    /// Gets whether registration is open, or null for the operator's default.
    /// </summary>
    public bool? RegistrationEnabled { get; init; }

    /// <summary>
    /// Gets whether a completed sign-up is held for review.
    /// </summary>
    public bool RegistrationReview { get; init; }

    /// <summary>
    /// Gets the invitation types the application allows: <c>JoinTenant</c> and <c>CreateTenant</c>. None allows both.
    /// </summary>
    public IList<string> InvitationTypes { get; init; } = [];

    /// <summary>
    /// Gets the personas an invitation may grant. None accepts any.
    /// </summary>
    public IList<string> Personas { get; init; } = [];

    /// <summary>
    /// Gets the default locale, or null for the operator's default.
    /// </summary>
    public string? DefaultLocale { get; init; }

    /// <summary>
    /// Gets the allowed locales, or none for the operator's default.
    /// </summary>
    public IList<string> AllowedLocales { get; init; } = [];

    /// <summary>
    /// Gets the organization names nobody can claim.
    /// </summary>
    public IList<string> ReservedOrganizationNames { get; init; } = [];

    /// <summary>
    /// Gets the legal terms, or null for no legal step.
    /// </summary>
    public LegalTermsArgs? Legal { get; init; }

    /// <summary>
    /// Gets the providers attested invitation exchange accepts. None keeps the legacy exchange.
    /// </summary>
    public IList<AttestedProviderArgs> AttestedProviders { get; init; } = [];

    /// <summary>
    /// Gets how Ante looks, or null to keep Ante's own look.
    /// </summary>
    public AnteBrandingArgs? Branding { get; init; }

    /// <summary>
    /// Builds the spec of the onboarding service, declaring what it refers to.
    /// </summary>
    /// <param name="context">The context of the Performance.</param>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec(PerformanceContext context) =>
        SpecExtensions.NewSpec()
            .Put("enabled", true)
            .Put("image", Image)
            .Put("port", Port)
            .Put("eventStore", EventStore)
            .Put("namespace", Namespace)
            .PutEnum("signUp", SignUp)
            .Put("registrationEnabled", RegistrationEnabled)
            .PutWhenTrue("registrationReview", RegistrationReview)
            .PutList("invitationTypes", InvitationTypes)
            .PutList("personas", Personas)
            .Put("defaultLocale", DefaultLocale)
            .PutList("allowedLocales", AllowedLocales)
            .PutList("reservedOrganizationNames", ReservedOrganizationNames)
            .PutObject("legal", Legal?.ToSpec(context) ?? SpecExtensions.NewSpec())
            .PutList("attestedProviders", AttestedProviders.Select(provider => provider.ToSpec()))
            .PutObject("branding", Branding?.ToSpec(context) ?? SpecExtensions.NewSpec());
}
