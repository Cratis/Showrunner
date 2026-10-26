// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// How Usher answers AuthProxy's on-behalf sessions: the grant check and the session notifications. Unconfigured,
/// the endpoints answer <c>404</c> and support can never sign in as a reporter.
/// </summary>
/// <remarks>
/// Pair it with AuthProxy's <c>OnBehalf:Attestation</c> section: the same issuer and audience, and the public half of
/// every key AuthProxy signs with.
/// </remarks>
public class OnBehalfOptions
{
    /// <summary>
    /// The smallest RSA key Usher accepts tokens signed with.
    /// </summary>
    public const int MinimumKeySize = 2048;

    /// <summary>
    /// Gets or sets the issuer AuthProxy writes to <c>iss</c>, its <c>OnBehalf:Attestation:Issuer</c>.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the audience AuthProxy writes to <c>aud</c>, its <c>OnBehalf:Attestation:Audience</c>.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the public keys AuthProxy signs with, selected by the token's <c>kid</c>.
    /// </summary>
    public IEnumerable<OnBehalfSigningKey> SigningKeys { get; set; } = [];

    /// <summary>
    /// Gets or sets the address AuthProxy calls the application on, its <c>Services:&lt;service&gt;:Backend:BaseUrl</c>.
    /// When set, the whole target a token is bound to (<c>htu</c>) is compared with it; when not, only the path is,
    /// because the application cannot tell the address AuthProxy used from behind a proxy.
    /// </summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets how far the clocks of AuthProxy and the application may disagree when a token's lifetime is checked.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the identity providers whose users are support agents rather than people. A session started by an
    /// actor from one of them is shown to the reporter as an agent's.
    /// </summary>
    public IEnumerable<string> AgentIdentityProviders { get; set; } = [];

    /// <summary>
    /// Gets a value indicating whether on-behalf sessions are configured: an issuer, an audience and at least one key.
    /// </summary>
    public bool IsEnabled =>
        !string.IsNullOrWhiteSpace(Issuer) && !string.IsNullOrWhiteSpace(Audience) && SigningKeys.Any();

    /// <summary>
    /// Gets a value indicating whether the section is either not configured at all or configured completely.
    /// </summary>
    public bool IsCompleteOrAbsent =>
        IsEnabled || (string.IsNullOrWhiteSpace(Issuer) && string.IsNullOrWhiteSpace(Audience) && !SigningKeys.Any());
}
