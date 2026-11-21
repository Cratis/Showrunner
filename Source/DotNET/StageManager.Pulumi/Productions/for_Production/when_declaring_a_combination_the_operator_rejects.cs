// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using Cratis.Showrunner.StageManager.Pulumi.given;
using OperatorProductionSpec = Cratis.Showrunner.StageManager.Productions.ProductionSpec;
using OperatorProductionValidator = Cratis.Showrunner.StageManager.Productions.ProductionValidator;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions.for_Production;

[Collection(PulumiSpecs.Collection)]
public class when_declaring_a_combination_the_operator_rejects : Specification
{
    IReadOnlyList<string> _problems;

    async Task Because()
    {
        var declared = await DeclaredResources.Run(() => _ = new Production(
            "production",
            new ProductionArgs
            {
                Namespace = PulumiSpecs.Namespace,
                Version = "19.164.2",
                Database = new ProductionDatabaseArgs { Kind = DatabaseKind.PostgreSql, Mode = DatabaseMode.Managed },
            }));
        _problems = OperatorProductionValidator.Validate(declared.SpecOf<OperatorProductionSpec>("Production"));
    }

    [Fact] void should_report_that_only_mongodb_can_be_managed() => _problems.ShouldContain(problem => problem.Contains("only MongoDB can be managed", StringComparison.Ordinal));
}
#endif
