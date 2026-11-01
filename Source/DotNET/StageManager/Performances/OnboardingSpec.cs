// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The onboarding service of an application: exactly one Ante per <see cref="Performance"/>.
/// </summary>
public class OnboardingSpec
{
    /// <summary>
    /// Gets or sets whether the application runs its own Ante. On when the model has onboarding.
    /// </summary>
    [Description("Whether the application runs its own Ante. On when the model has onboarding.")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the Ante image.
    /// </summary>
    [Description("The Ante image.")]
    public string Image { get; set; } = "cratis/ante:latest";

    /// <summary>
    /// Gets or sets the port Ante listens on.
    /// </summary>
    [Description("The port Ante listens on.")]
    public int Port { get; set; } = 8080;

    /// <summary>
    /// Gets or sets Ante's own event store. Defaults to ante-NAME, so two Performances on one Production never share a store.
    /// </summary>
    [Description("Ante's own event store. Defaults to ante-NAME, so two Performances on one Production never share a store.")]
    public string? EventStore { get; set; }

    /// <summary>
    /// Gets or sets the Chronicle namespace Ante runs in.
    /// </summary>
    [Description("The Chronicle namespace Ante runs in.")]
    public string Namespace { get; set; } = "Default";

    /// <summary>
    /// Gets or sets what self-service sign-up does.
    /// </summary>
    [Description("CreateTenant or JoinTenant.")]
    public SignUpMode SignUp { get; set; } = SignUpMode.CreateTenant;

    /// <summary>
    /// Gets or sets whether registration is open.
    /// </summary>
    [Description("Whether registration is open. Always closed when signUp is JoinTenant.")]
    public bool RegistrationEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the invitation types the application allows: JoinTenant and CreateTenant.
    /// </summary>
    [Description("The invitation types the application allows: JoinTenant, CreateTenant. Empty allows both.")]
    public IList<string> InvitationTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets the personas an invitation may grant.
    /// </summary>
    [Description("The personas an invitation may grant. Empty accepts any.")]
    public IList<string> Personas { get; set; } = [];

    /// <summary>
    /// Gets or sets the default locale.
    /// </summary>
    [Description("The default locale.")]
    public string DefaultLocale { get; set; } = "en";

    /// <summary>
    /// Gets or sets the allowed locales.
    /// </summary>
    [Description("The allowed locales.")]
    public IList<string> AllowedLocales { get; set; } = ["en"];

    /// <summary>
    /// Gets or sets the legal terms. Omit for no legal step.
    /// </summary>
    [Description("The legal terms. Omit for no legal step.")]
    public LegalTermsSpec? Legal { get; set; }

    /// <summary>
    /// Gets or sets the providers attested invitation exchange accepts. When empty, Ante keeps the legacy exchange.
    /// </summary>
    [Description("The providers attested invitation exchange accepts. When empty, Ante keeps the legacy exchange.")]
    public IList<AttestedProvider> AttestedProviders { get; set; } = [];
}
