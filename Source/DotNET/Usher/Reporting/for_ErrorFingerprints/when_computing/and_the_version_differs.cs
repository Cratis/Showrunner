// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.given;

namespace Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.when_computing;

public class and_the_version_differs : an_error
{
    ErrorFingerprint _inThisVersion;
    ErrorFingerprint _inTheNextVersion;

    void Because()
    {
        _inThisVersion = FingerprintOf("Invoice not found");
        _inTheNextVersion = FingerprintOf("Invoice not found", version: "1.2.4");
    }

    [Fact] void should_give_another_fingerprint() => _inTheNextVersion.ShouldNotEqual(_inThisVersion);
}
#endif
