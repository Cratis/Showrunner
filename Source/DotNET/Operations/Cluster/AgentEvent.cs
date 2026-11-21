// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// An event about a Pod, the evidence that a required Secret is missing.
/// </summary>
/// <param name="InvolvedUid">The UID of the object it is about.</param>
/// <param name="InvolvedKind">The kind of the object it is about.</param>
/// <param name="Reason">The reason, for example <c>FailedMount</c>.</param>
/// <param name="Count">How many times it was observed.</param>
/// <param name="Latest">When it was last observed; <see langword="null"/> when unknown.</param>
/// <param name="Message">The message.</param>
public sealed record AgentEvent(string InvolvedUid, string InvolvedKind, string Reason, int Count, DateTimeOffset? Latest, string Message);
