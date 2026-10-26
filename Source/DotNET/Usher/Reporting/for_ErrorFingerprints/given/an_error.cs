// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.given;

/// <summary>
/// The fingerprint algorithm and an error the specifications vary one part of.
/// </summary>
public class an_error : Specification
{
    protected static readonly ApplicationVersion Version = "1.2.3";
    protected static readonly SliceLocation Location = new("Invoicing", "Invoices", "SendInvoice");
    protected static readonly ErrorType Type = "Ledger.InvoiceNotFound";
    protected static readonly StackFrame TopFrame = new("Ledger.Invoicing.SendInvoice.Handle", "/build/Invoicing/SendInvoice.cs", 42);

    protected ErrorFingerprints _fingerprints;

    void Establish() => _fingerprints = new();

    /// <summary>
    /// Computes the fingerprint of the error, varying what is given.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="stackFrames">The stack frames; the top frame when not given.</param>
    /// <param name="version">The application version; <see cref="Version"/> when not given.</param>
    /// <param name="location">The location; <see cref="Location"/> when not given.</param>
    /// <returns>The fingerprint.</returns>
    protected ErrorFingerprint FingerprintOf(
        ErrorMessage message,
        IEnumerable<StackFrame>? stackFrames = null,
        ApplicationVersion? version = null,
        SliceLocation? location = null) =>
        _fingerprints.Compute(version ?? Version, location ?? Location, Type, message, stackFrames ?? [TopFrame]);
}
#endif
