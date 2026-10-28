// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Contracts.Reporting.for_SliceLocation;

/// <summary>
/// A location Arc cannot resolve travels as empty parts, never as absent ones, so a consumer that receives one -
/// for example from the frontend or after deserializing a payload - must be able to recognize it as not set by value.
/// </summary>
public class when_building_a_location_from_empty_parts : Specification
{
    SliceLocation _location = new(ModuleName.NotSet, FeatureName.NotSet, SliceName.NotSet);

    void Because() => _location = new SliceLocation(string.Empty, string.Empty, string.Empty);

    [Fact] void should_be_the_location_that_is_not_set() => Assert.Equal(SliceLocation.NotSet, _location);
}
#endif
