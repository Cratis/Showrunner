// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Direct's answer to one request.
/// </summary>
/// <typeparam name="T">The response type.</typeparam>
/// <param name="Outcome">How it came out.</param>
/// <param name="Value">The response, for <see cref="DirectOutcome.Ok"/>.</param>
/// <param name="Reason">Why, for anything else - never a credential.</param>
public sealed record DirectAnswer<T>(DirectOutcome Outcome, T? Value, string Reason)
{
    /// <summary>
    /// Gets a value indicating whether the request succeeded with a response.
    /// </summary>
    public bool IsOk => Outcome == DirectOutcome.Ok && Value is not null;
}
