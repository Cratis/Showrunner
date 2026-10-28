// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.given;

namespace Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.when_computing;

public class and_the_same_error_happens_with_other_ids : an_error
{
    ErrorFingerprint _first;
    ErrorFingerprint _again;
    ErrorFingerprint _withOtherIds;

    void Because()
    {
        _first = FingerprintOf("Invoice 3f2504e0-4f89-11d3-9a0c-0305e82c3301 was not found after 3 attempts");
        _again = FingerprintOf("Invoice 3f2504e0-4f89-11d3-9a0c-0305e82c3301 was not found after 3 attempts");
        _withOtherIds = FingerprintOf("Invoice 9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d was not found after 5 attempts");
    }

    [Fact] void should_give_the_same_fingerprint_again() => _again.ShouldEqual(_first);
    [Fact] void should_give_the_same_fingerprint_with_other_ids_and_numbers() => _withOtherIds.ShouldEqual(_first);
    [Fact] void should_be_a_lowercase_hexadecimal_sha256() => _first.Value.ShouldMatch(value => value.Length == 64 && value.All(c => char.IsAsciiHexDigitLower(c) || char.IsAsciiDigit(c)));
}
#endif
