// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// Everything the offline decision is made from.
/// </summary>
/// <param name="Configuration">The signed configuration the instance holds - <see langword="null"/> for none.</param>
/// <param name="SigningKey">The instance's base64 signing key.</param>
/// <param name="InstanceName">The instance's name.</param>
/// <param name="Remedy">The remedy asked for.</param>
/// <param name="Now">The current time.</param>
/// <param name="DurableQueue">Whether the outbox is on durable storage.</param>
/// <param name="FencingCertain">Whether the instance is certain it holds the current epoch - false once Direct answered
/// 409, and whenever the claim could not be confirmed within its lease.</param>
/// <param name="TargetState">What was last observed of the remedy's verification check.</param>
/// <param name="EvidenceObservedAt">When that observation was made.</param>
/// <param name="PreviousAttempts">When earlier attempts at this remedy started, online or offline.</param>
public record OfflineRemedyContext(
    SignedWatchdogConfiguration? Configuration,
    string SigningKey,
    string InstanceName,
    string Remedy,
    DateTimeOffset Now,
    bool DurableQueue,
    bool FencingCertain,
    ObservedCheckState TargetState,
    DateTimeOffset EvidenceObservedAt,
    IReadOnlyList<DateTimeOffset> PreviousAttempts);
