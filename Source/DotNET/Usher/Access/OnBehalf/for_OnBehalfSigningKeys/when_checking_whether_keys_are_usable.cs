// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using Cratis.Arc.Identity;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfSigningKeys;

public class when_checking_whether_keys_are_usable : Specification
{
    bool _publicKey;
    bool _privateKey;
    bool _smallKey;
    bool _duplicateKeyId;
    bool _notAKey;
    bool _none;

    void Because()
    {
        using var rsa = RSA.Create(2048);
        using var small = RSA.Create(1024);
        var publicPem = rsa.ExportSubjectPublicKeyInfoPem();
        _publicKey = Usable(new OnBehalfOfSigningKey { KeyId = "a", PublicKeyPem = publicPem });
        _privateKey = Usable(new OnBehalfOfSigningKey { KeyId = "a", PublicKeyPem = rsa.ExportPkcs8PrivateKeyPem() });
        _smallKey = Usable(new OnBehalfOfSigningKey { KeyId = "a", PublicKeyPem = small.ExportSubjectPublicKeyInfoPem() });
        _duplicateKeyId = Usable(new OnBehalfOfSigningKey { KeyId = "a", PublicKeyPem = publicPem }, new OnBehalfOfSigningKey { KeyId = "a", PublicKeyPem = publicPem });
        _notAKey = Usable(new OnBehalfOfSigningKey { KeyId = "a", PublicKeyPem = "not a key" });
        _none = Usable();
    }

    [Fact] void should_accept_a_public_key() => _publicKey.ShouldBeTrue();
    [Fact] void should_refuse_a_private_key() => _privateKey.ShouldBeFalse();
    [Fact] void should_refuse_a_key_smaller_than_2048_bits() => _smallKey.ShouldBeFalse();
    [Fact] void should_refuse_a_key_identifier_used_twice() => _duplicateKeyId.ShouldBeFalse();
    [Fact] void should_refuse_something_that_is_not_a_key() => _notAKey.ShouldBeFalse();
    [Fact] void should_accept_no_keys_at_all() => _none.ShouldBeTrue();

    static bool Usable(params OnBehalfOfSigningKey[] keys) => OnBehalfSigningKeys.AreUsable(keys);
}
#endif
