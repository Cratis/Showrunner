// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Collections.Immutable;
using System.Text.Json;
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorProductionSpec = Cratis.Showrunner.StageManager.Productions.ProductionSpec;

namespace Cratis.Showrunner.StageManager.Pulumi.for_OperatorJson;

public class when_reading_a_property_the_operator_does_not_have : Specification
{
    Exception _unknown;
    Exception _wrongCase;

    void Because()
    {
        _unknown = Catch.Exception(() => OperatorJson.Deserialize<OperatorProductionSpec>(ImmutableDictionary<string, object>.Empty.Add("version", "1").Add("replicaz", 2d)));
        _wrongCase = Catch.Exception(() => OperatorJson.Deserialize<OperatorProductionSpec>(ImmutableDictionary<string, object>.Empty.Add("Version", "1")));
    }

    [Fact] void should_reject_the_unknown_property() => _unknown.ShouldBeOfExactType<JsonException>();
    [Fact] void should_reject_a_property_spelled_with_another_case() => _wrongCase.ShouldBeOfExactType<JsonException>();
}
#endif
