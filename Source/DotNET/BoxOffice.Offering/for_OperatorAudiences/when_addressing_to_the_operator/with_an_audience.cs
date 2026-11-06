// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration;

namespace Cratis.Showrunner.BoxOffice.Offering.for_OperatorAudiences.when_addressing_to_the_operator;

public class with_an_audience : Specification
{
    IDisposable? _scope;

    void Because() => _scope = new OfferingOptions { OperatorAudience = "direct" }.AddressedToOperator();

    void Destroy() => _scope?.Dispose();

    [Fact] void should_open_a_scope() => _scope.ShouldNotBeNull();
}
#endif
