// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;
using k8s.Models;

namespace Cratis.Showrunner.StageOperator.given;

/// <summary>Builds applications, observed states and contexts for the specs.</summary>
public static class Applications
{
    public const string OldImage = "registry.example.invalid/orders@sha256:aaaa";
    public const string NewImage = "registry.example.invalid/orders@sha256:bbbb";
    public const string BrokenImage = "registry.example.invalid/orders@sha256:cccc";

    public static readonly DateTimeOffset Now = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    public static StageApplication Create(
        string version = "1.1.0",
        string image = NewImage,
        StageApplicationStatus? status = null,
        bool permanent = true,
        DateTimeOffset? until = null,
        bool withAuthentication = true,
        DateTimeOffset? deletedAt = null,
        bool withFinalizer = true,
        string? package = null) => new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = "orders",
                NamespaceProperty = "published",
                Uid = "11111111-1111-1111-1111-111111111111",
                Generation = 3,
                Finalizers = withFinalizer ? [StageApplication.RetentionFinalizer] : null,
                DeletionTimestamp = deletedAt?.UtcDateTime
            },
            Spec = new StageApplicationSpec
            {
                PublicationId = "22222222-2222-2222-2222-222222222222",
                Organization = "Example Org",
                PublishedName = "orders",
                Version = version,
                Image = image,
                Package = package is null ? null : new StageApplicationPackage { ConfigMapRef = package },
                Lifetime = permanent ? new StageApplicationLifetime { Permanent = true } : new StageApplicationLifetime { Until = Rfc3339.Format(until ?? Now.AddDays(1)) },
                Authentication = withAuthentication ? new StageApplicationAuthentication { ProxyImage = "registry.example.invalid/proxy:1", ConfigSecretRef = "orders-auth", Port = 8080 } : null
            },
            Status = status
        };

    public static StageApplicationStatus Running(string version = "1.0.0", string image = OldImage, string? package = null) => new()
    {
        Phase = StageApplicationPhases.Running,
        RunningVersion = version,
        RunningImage = image,
        RunningPackage = package,
        Versions = [new StageApplicationVersion { Version = version, Image = image, Package = package, Outcome = StageRolloutOutcomes.RolledOut, RolledOutAt = Rfc3339.Format(Now.AddDays(-1)) }]
    };

    public static ObservedState Ready(string image, string? package = null) => new(new ObservedDeployment(image, 1, 1, 1, 5, 5, false, package), true);

    public static ObservedState Updating(string newImage) => new(new ObservedDeployment(newImage, 2, 1, 1, 6, 6, false), true);

    public static PlanningContext Context(StageApplication application, ObservedState? observed = null, DateTimeOffset? now = null, StageOperatorOptions? options = null) =>
        new(application, observed ?? new ObservedState(null, false), now ?? Now, options ?? new StageOperatorOptions());
}
#endif
