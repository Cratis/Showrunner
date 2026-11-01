// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Metering;

namespace Cratis.Showrunner.StageOperator.for_QuantityParser;

public class when_parsing : Specification
{
    [Fact] void should_parse_nanocores() => Assert.Equal(0.25, QuantityParser.Parse("250000000n"), 6);
    [Fact] void should_parse_millicores() => Assert.Equal(0.25, QuantityParser.Parse("250m"), 6);
    [Fact] void should_parse_whole_cores() => Assert.Equal(2, QuantityParser.Parse("2"), 6);
    [Fact] void should_parse_kibibytes() => Assert.Equal(2048, QuantityParser.Parse("2Ki"), 6);
    [Fact] void should_parse_mebibytes() => Assert.Equal(512d * 1024 * 1024, QuantityParser.Parse("512Mi"), 6);
    [Fact] void should_parse_decimal_megabytes() => Assert.Equal(5e6, QuantityParser.Parse("5M"), 6);
    [Fact] void should_treat_nothing_as_zero() => Assert.Equal(0, QuantityParser.Parse(null));
    [Fact] void should_treat_garbage_as_zero() => Assert.Equal(0, QuantityParser.Parse("lots"));
}
#endif
