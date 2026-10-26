// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using Cratis.Arc;
using Cratis.Arc.Identity;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfConfiguration;

public class when_validating : Specification
{
    OnBehalfConfiguration _configuration;
    string _publicKey;
    bool _disabled;
    bool _complete;
    bool _untrustedHeaders;
    bool _withoutIssuer;
    bool _withoutKeys;
    bool _withPrivateKey;

    void Establish()
    {
        _configuration = new();
        using var rsa = RSA.Create(2048);
        _publicKey = rsa.ExportSubjectPublicKeyInfoPem();
    }

    void Because()
    {
        using var rsa = RSA.Create(2048);
        _disabled = Valid(new ArcOptions());
        _complete = Valid(Options());
        _untrustedHeaders = Valid(Options(trust: false));
        _withoutIssuer = Valid(Options(issuer: string.Empty));
        _withoutKeys = Valid(Options(keys: []));
        _withPrivateKey = Valid(Options(keys: [new() { KeyId = "a", PublicKeyPem = rsa.ExportPkcs8PrivateKeyPem() }]));
    }

    [Fact] void should_accept_on_behalf_forwards_turned_off() => _disabled.ShouldBeTrue();
    [Fact] void should_accept_a_complete_configuration() => _complete.ShouldBeTrue();
    [Fact] void should_refuse_untrusted_forwarded_identity_headers() => _untrustedHeaders.ShouldBeFalse();
    [Fact] void should_refuse_a_missing_issuer() => _withoutIssuer.ShouldBeFalse();
    [Fact] void should_refuse_no_keys() => _withoutKeys.ShouldBeFalse();
    [Fact] void should_refuse_a_private_key() => _withPrivateKey.ShouldBeFalse();

    bool Valid(ArcOptions options) => _configuration.Validate(null, options).Succeeded;

    ArcOptions Options(bool trust = true, string issuer = "https://auth.example.com", IList<OnBehalfOfSigningKey>? keys = null) => new()
    {
        TrustForwardedIdentityHeaders = trust,
        OnBehalfOf = new()
        {
            Enabled = true,
            Issuer = issuer,
            Audience = "portal",
            SigningKeys = keys ?? [new() { KeyId = "a", PublicKeyPem = _publicKey }]
        }
    };
}
#endif
