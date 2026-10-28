// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.given;

namespace Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.when_computing;

public class and_the_slice_differs : an_error
{
    ErrorFingerprint _inThisSlice;
    ErrorFingerprint _inAnotherSlice;

    void Because()
    {
        _inThisSlice = FingerprintOf("Invoice not found");
        _inAnotherSlice = FingerprintOf("Invoice not found", location: new("Invoicing", "Invoices", "CancelInvoice"));
    }

    [Fact] void should_give_another_fingerprint() => _inAnotherSlice.ShouldNotEqual(_inThisSlice);
}
#endif
