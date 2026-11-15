// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyValidator;

public class when_validating_the_storage : Specification
{
    static IReadOnlyList<UnderstudyProblem> Validate(UnderstudyStorage storage)
    {
        var spec = UnderstudySamples.Spec();
        spec.Storage = storage;
        return UnderstudyValidator.Validate(spec);
    }

    [Fact] void should_refuse_an_unknown_provider() => Validate(new() { Provider = "Floppy", Bucket = "b", CredentialsSecretName = "c" }).Select(problem => problem.Reason).ShouldContain("UnsupportedProvider");
    [Fact] void should_ask_a_volume_for_its_claim() => Validate(new() { Provider = ObjectStorageProviders.Volume }).Select(problem => problem.Reason).ShouldContain("VolumeClaimMissing");
    [Fact] void should_not_ask_a_volume_for_credentials() => Validate(new() { Provider = ObjectStorageProviders.Volume, VolumeClaimName = "backups" }).ShouldBeEmpty();
    [Fact] void should_accept_azure_blob() => Validate(new() { Provider = ObjectStorageProviders.AzureBlob, Bucket = "b", CredentialsSecretName = "azure" }).ShouldBeEmpty();
}
#endif
