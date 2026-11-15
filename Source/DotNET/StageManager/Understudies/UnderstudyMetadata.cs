// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// Labels and owner references shared by everything an <see cref="Understudy"/> owns. Owned resources are removed with the Understudy; the backups in object storage stay.
/// </summary>
public static class UnderstudyMetadata
{
    /// <summary>
    /// The label that ties everything to the Understudy that owns it.
    /// </summary>
    public const string UnderstudyLabel = "showrunner.cratis.io/understudy";

    /// <summary>
    /// The label that tells what a job does, one of the kinds in <see cref="Jobs.UnderstudyJobKinds"/>.
    /// </summary>
    public const string KindLabel = "showrunner.cratis.io/understudy-job";

    /// <summary>
    /// The annotation that asks for a snapshot of the whole Vault. A new value asks again.
    /// </summary>
    public const string VaultSnapshotAnnotation = "showrunner.cratis.io/vault-snapshot";

    /// <summary>
    /// Creates the metadata of an owned resource.
    /// </summary>
    /// <param name="understudy">The Understudy.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="kind">What the resource does, one of the kinds in <see cref="Jobs.UnderstudyJobKinds"/>.</param>
    /// <returns>The metadata.</returns>
    public static V1ObjectMeta Create(Understudy understudy, string name, string kind) => new()
    {
        Name = name,
        NamespaceProperty = understudy.Metadata.NamespaceProperty,
        Labels = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["app.kubernetes.io/name"] = "understudy",
            ["app.kubernetes.io/instance"] = understudy.Metadata.Name,
            ["app.kubernetes.io/part-of"] = "showrunner",
            ["app.kubernetes.io/managed-by"] = "stage-manager",
            [UnderstudyLabel] = understudy.Metadata.Name,
            [KindLabel] = kind
        },
        OwnerReferences =
        [
            new V1OwnerReference
            {
                ApiVersion = $"{StageManagerApi.Group}/{StageManagerApi.Version}",
                Kind = "Understudy",
                Name = understudy.Metadata.Name,
                Uid = understudy.Metadata.Uid,
                Controller = true,
                BlockOwnerDeletion = true
            }
        ]
    };
}
