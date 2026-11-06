// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Offering.Declaration;

namespace Cratis.Showrunner.BoxOffice.Offering.for_OperatorAudiences.when_addressing_to_the_operator;

public class without_options : Specification
{
    IDisposable? _none;
    IDisposable? _unset;
    IDisposable? _blank;

    void Because()
    {
        _none = ((OfferingOptions?)null).AddressedToOperator();
        _unset = new OfferingOptions().AddressedToOperator();
        _blank = new OfferingOptions { OperatorAudience = "  " }.AddressedToOperator();
    }

    [Fact] void should_address_nothing_without_options() => _none.ShouldBeNull();
    [Fact] void should_address_nothing_without_an_audience() => _unset.ShouldBeNull();
    [Fact] void should_address_nothing_with_a_blank_audience() => _blank.ShouldBeNull();
}
#endif
