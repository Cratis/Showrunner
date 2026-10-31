// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_BoxOfficeOfferingServiceCollectionExtensions.when_adding_an_offering;

public class that_requires_a_missing_module : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => new ServiceCollection().AddBoxOfficeOffering(offering => offering
        .Product(ProductId.New())
        .Module("analytics", "Analytics", module => module.Requires("core"))));

    [Fact] void should_refuse_to_start() => _error.ShouldBeOfExactType<InvalidOffering>();
}
#endif
