// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A claim the signed-in caller must carry before AuthProxy forwards anything.
/// </summary>
public class AuthProxyRequiredClaim
{
    /// <summary>
    /// Gets or sets the claim type, for example <c>urn:github:organization</c>.
    /// </summary>
    [Required]
    [Description("The claim type, for example urn:github:organization or urn:github:team.")]
    public string Claim { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the values of which the caller needs one.
    /// </summary>
    [Description("The values of which the claim must carry one. Empty requires only that the claim is present.")]
    public IList<string> AnyOf { get; set; } = [];
}
