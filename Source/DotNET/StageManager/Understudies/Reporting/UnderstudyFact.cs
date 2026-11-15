// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Reporting;

/// <summary>
/// One fact Stage Manager publishes: a public event of <c>Cratis.Showrunner.Understudy.Contracts</c> with the identity that makes its redelivery safe.
/// </summary>
/// <param name="Id">The identity of the fact. The same fact always has the same id, so a consumer that receives it twice can recognize it.</param>
/// <param name="Type">The name of the event type, such as <c>BackupTaken</c>.</param>
/// <param name="Payload">The event.</param>
/// <param name="Warning">Whether the fact is a failure, which Kubernetes shows as a warning.</param>
public record UnderstudyFact(string Id, string Type, object Payload, bool Warning);
