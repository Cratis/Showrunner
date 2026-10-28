// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.given;

namespace Cratis.Showrunner.Usher.Reporting.for_ErrorFingerprints.when_computing;

public class and_the_frames_differ_only_where_they_were_served_or_below_the_top : an_error
{
    ErrorFingerprint _servedFromOneHost;
    ErrorFingerprint _servedFromAnotherHost;
    ErrorFingerprint _withOtherFramesBelow;
    ErrorFingerprint _withAnotherTopLine;

    void Because()
    {
        _servedFromOneHost = FingerprintOf("Failed", [new("pay", "https://ledger.example.com/assets/index-4f1c.js?v=1", 12)]);
        _servedFromAnotherHost = FingerprintOf("Failed", [new("pay", "https://fjord.example.org/assets/index-4f1c.js", 12)]);
        _withOtherFramesBelow = FingerprintOf("Failed", [new("pay", "https://ledger.example.com/assets/index-4f1c.js", 12), new("click", "react-dom.js", 900)]);
        _withAnotherTopLine = FingerprintOf("Failed", [new("pay", "https://ledger.example.com/assets/index-4f1c.js", 13)]);
    }

    [Fact] void should_give_the_same_fingerprint_from_another_host() => _servedFromAnotherHost.ShouldEqual(_servedFromOneHost);
    [Fact] void should_give_the_same_fingerprint_with_other_frames_below_the_top() => _withOtherFramesBelow.ShouldEqual(_servedFromOneHost);
    [Fact] void should_give_another_fingerprint_for_another_top_line() => _withAnotherTopLine.ShouldNotEqual(_servedFromOneHost);
}
#endif
