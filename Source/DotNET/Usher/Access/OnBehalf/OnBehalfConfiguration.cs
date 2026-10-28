// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Refuses to start a host whose Arc on-behalf configuration Usher can not answer AuthProxy's on-behalf sessions with:
/// on-behalf forwards enabled without trusting the forwarded identity headers, without an issuer or an audience, or with
/// a key that is missing, private, smaller than 2048 bits or used twice.
/// </summary>
/// <remarks>
/// Arc registers <see cref="ArcOptions"/> with <c>ValidateOnStart</c>, so a misconfiguration fails the host at start
/// rather than refusing every session later.
/// </remarks>
public class OnBehalfConfiguration : IValidateOptions<ArcOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, ArcOptions options)
    {
        var onBehalfOf = options.OnBehalfOf;
        if (onBehalfOf?.Enabled != true)
        {
            return ValidateOptionsResult.Success;
        }

        List<string> failures = [];
        if (!options.TrustForwardedIdentityHeaders)
        {
            failures.Add("Usher's on-behalf sessions need Arc to trust AuthProxy's forwarded identity headers: set Cratis:Arc:TrustForwardedIdentityHeaders.");
        }

        if (string.IsNullOrWhiteSpace(onBehalfOf.Issuer) || string.IsNullOrWhiteSpace(onBehalfOf.Audience))
        {
            failures.Add("Usher's on-behalf sessions need the issuer and the audience of AuthProxy's OnBehalf:Attestation: set Cratis:Arc:OnBehalfOf:Issuer and Audience.");
        }

        if (!onBehalfOf.SigningKeys.Any() || !OnBehalfSigningKeys.AreUsable(onBehalfOf.SigningKeys))
        {
            failures.Add("Every Cratis:Arc:OnBehalfOf:SigningKeys entry needs a unique KeyId and the PEM of an RSA public key of at least 2048 bits; never a private key. At least one is needed.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
