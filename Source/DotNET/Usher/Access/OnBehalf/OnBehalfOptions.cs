// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// What Usher adds to Arc's on-behalf configuration to answer AuthProxy's on-behalf sessions.
/// </summary>
/// <remarks>
/// On-behalf sessions are on exactly when Arc accepts on-behalf forwards: <c>Cratis:Arc:OnBehalfOf</c> enabled, with
/// the issuer, the audience and the public keys of AuthProxy's <c>OnBehalf:Attestation</c>. Usher verifies the tokens of
/// AuthProxy's grant check and session notifications with that same configuration, so the keys are pinned once.
/// </remarks>
public class OnBehalfOptions
{
    /// <summary>
    /// Gets or sets the address AuthProxy calls the application on, its <c>Services:&lt;service&gt;:Backend:BaseUrl</c>.
    /// When set, the whole target a call's token is bound to (<c>htu</c>) is compared with it; when not, only the path
    /// is, because the application cannot tell the address AuthProxy used from behind a proxy.
    /// </summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the identity providers whose users are support agents rather than people. A session started by an
    /// actor from one of them is shown to the reporter as an agent's.
    /// </summary>
    public IEnumerable<string> AgentIdentityProviders { get; set; } = [];
}
