// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.for_ErrorMessageNormalization.when_normalizing;

public class and_it_has_numbers : Specification
{
    string _normalized;

    void Because() => _normalized = ErrorMessageNormalization.Normalize("Retried 3 times in 1,250.5 ms at 2026-10-26T12:00:00Z for item42");

    [Fact] void should_replace_each_number_with_a_number_placeholder() => _normalized.ShouldEqual("Retried <n> times in <n> ms at <n>-<n>-<n>T<n>:<n>:<n>Z for item<n>");
}
#endif
