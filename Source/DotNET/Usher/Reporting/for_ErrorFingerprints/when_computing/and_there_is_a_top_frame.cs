// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;

namespace Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.when_computing;

/// <summary>
/// Pins the algorithm to a known value, so a change to it can never pass unnoticed: fingerprints are stored forever.
/// </summary>
public class and_there_is_a_top_frame : Specification
{
    ErrorFingerprint _fingerprint;

    void Because() => _fingerprint = new ErrorFingerprints().Compute(
        "1.2.3",
        new("Invoicing", "Invoices", "SendInvoice"),
        "System.NullReferenceException",
        "Object reference not set to an instance of an object.",
        [new("Ledger.Invoicing.Send", "SendInvoice.cs", 42), new("Ledger.Invoicing.Caller", "Caller.cs", 7)]);

    [Fact] void should_hash_the_version_location_type_message_and_top_frame() => _fingerprint.Value.ShouldEqual("d983a0fd844c62860583a8f34a29320cc20ac59c017afba57d1417020e704524");
}
#endif
