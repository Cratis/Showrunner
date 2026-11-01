// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// The exception that is thrown when the event log did not take a fact about an email.
/// </summary>
/// <param name="eventType">The fact.</param>
/// <param name="about">Whatever the email is about.</param>
/// <param name="result">What the event log answered.</param>
public class EmailNotQueued(Type eventType, EventSourceId about, AppendResult result)
    : Exception($"The event log did not take {eventType.Name} for '{about}': {result.Errors.Count()} error(s) and {result.ConstraintViolations.Count()} constraint violation(s).");
