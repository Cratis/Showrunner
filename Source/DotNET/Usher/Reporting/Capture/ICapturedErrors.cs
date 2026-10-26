// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Reporting.Capture;

/// <summary>
/// Defines the short-lived store of errors the backend captured, by the correlation id of the failing operation.
/// </summary>
public interface ICapturedErrors
{
    /// <summary>
    /// Keeps a captured error. The first error captured for a correlation id is kept.
    /// </summary>
    /// <param name="owner">The <see cref="ErrorOwner"/> the error happened for.</param>
    /// <param name="correlationId">The <see cref="Contracts.Reporting.CorrelationId"/> of the failing operation.</param>
    /// <param name="error">The <see cref="CapturedError"/>.</param>
    void Capture(ErrorOwner owner, CorrelationId correlationId, CapturedError error);

    /// <summary>
    /// Gets a captured error that has not expired, for the owner it happened for only.
    /// </summary>
    /// <param name="owner">The <see cref="ErrorOwner"/> asking.</param>
    /// <param name="correlationId">The <see cref="Contracts.Reporting.CorrelationId"/> of the failing operation.</param>
    /// <param name="error">The <see cref="CapturedError"/>, when there is one.</param>
    /// <returns>True when the owner has a captured error for the correlation id.</returns>
    bool TryGet(ErrorOwner owner, CorrelationId correlationId, [NotNullWhen(true)] out CapturedError? error);
}
