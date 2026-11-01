// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// An identity provider whose sign-ins Ante accepts on AuthProxy's signed word, for attested invitation exchange.
/// </summary>
public class AttestedProvider
{
    /// <summary>
    /// Gets or sets the canonical provider key: the lowercase identity.
    /// </summary>
    [Required]
    [Description("The canonical provider key: the lowercase identity.")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exact authority the provider signs in with.
    /// </summary>
    [Required]
    [Description("The exact authority (issuer) of the provider.")]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assurance values Ante accepts from the provider.
    /// </summary>
    [Description("The provider-derived assurance values Ante accepts.")]
    public IList<string> AcceptableAssurances { get; set; } = [];
}
