// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorUnderstudySpec = Cratis.Showrunner.StageManager.Understudies.UnderstudySpec;
using OperatorUnderstudyValidator = Cratis.Showrunner.StageManager.Understudies.UnderstudyValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Understudies.for_Understudy;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_backups_that_share_the_bucket_with_the_keys : Specification
{
    IReadOnlyList<string> _problems;

    async Task Because()
    {
        var storage = new UnderstudyStorageArgs { Endpoint = "https://objects.example.com", Bucket = "shared", CredentialsSecretName = "credentials" };
        var declared = await DeclaredResources.Run(() => _ = new Understudy(
            "understudy",
            new UnderstudyArgs
            {
                Namespace = PulumiSpecs.Namespace,
                ProductionName = "keldakraft",
                Storage = storage,
                Keys = new UnderstudyKeysArgs
                {
                    Storage = storage,
                    CertificateSecretName = "certificate",
                    VaultTokenSecret = new SecretReferenceArgs { Name = "token" },
                },
            }));
        _problems = [.. OperatorUnderstudyValidator.Validate(declared.SpecOf<OperatorUnderstudySpec>("Understudy")).Select(problem => problem.Message)];
    }

    [Fact] void should_be_refused_by_the_operator() => _problems.ShouldNotBeEmpty();
}
#endif
