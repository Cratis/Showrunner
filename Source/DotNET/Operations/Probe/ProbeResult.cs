// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Probe;

/// <summary>
/// The outcome of one probe request.
/// </summary>
/// <param name="Target">The name of the target.</param>
/// <param name="Check">The id of the check.</param>
/// <param name="Ok">Whether the endpoint answered with the status it must.</param>
/// <param name="Status">The HTTP status, or <see langword="null"/> when there was no answer.</param>
/// <param name="Error">A stable error code when there was no answer. It never carries a message: those can contain URLs.</param>
/// <param name="Milliseconds">How long the request took.</param>
public sealed record ProbeResult(string Target, string Check, bool Ok, int? Status, string? Error, double Milliseconds)
{
    /// <summary>
    /// Gets the failure as it is told to a human: <c>target/check: status or error</c>.
    /// </summary>
    public string Description => $"{Target}/{Check}: {(Status?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? Error) ?? "request failed"}";
}
