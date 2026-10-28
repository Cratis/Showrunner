// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.for_ErrorMessageNormalization.when_normalizing;

public class and_it_has_guids : Specification
{
    string _normalized;

    void Because() => _normalized = ErrorMessageNormalization.Normalize("Order 3f2504e0-4f89-11d3-9a0c-0305e82c3301, {9B1DEB4D-3B7D-4BAD-9BDD-2B0D7B3DCB6D} and 9b1deb4d3b7d4bad9bdd2b0d7b3dcb6d failed");

    [Fact] void should_replace_each_guid_with_an_id_placeholder() => _normalized.ShouldEqual("Order <id>, <id> and <id> failed");
}
#endif
