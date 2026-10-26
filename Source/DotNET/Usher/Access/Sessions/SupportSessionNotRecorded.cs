// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// The exception that is thrown when the event log refuses the start or end of a session in which support signs in
/// as the reporter. A session is a security fact: it is never reported as recorded when it was not.
/// </summary>
/// <param name="eventType">The type of the event the event log refused.</param>
/// <param name="issueId">The issue.</param>
/// <param name="namespace">The namespace of the issue.</param>
/// <param name="result">The result of the append.</param>
public class SupportSessionNotRecorded(Type eventType, IssueId issueId, EventStoreNamespaceName @namespace, AppendResult result)
    : Exception($"'{eventType.Name}' on issue '{issueId}' in namespace '{@namespace}' was not recorded: {result.Errors.Count()} error(s) and {result.ConstraintViolations.Count()} constraint violation(s).");
