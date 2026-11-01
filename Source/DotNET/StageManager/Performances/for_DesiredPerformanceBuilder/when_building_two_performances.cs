// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_DesiredPerformanceBuilder;

public class when_building_two_performances : Specification
{
    string _first;
    string _second;

    void Because()
    {
        _first = AnteContract.EnvironmentVariables(PerformanceSamples.Performance("crm"))["Ante__Onboarding__EventStore"];
        _second = AnteContract.EnvironmentVariables(PerformanceSamples.Performance("billing"))["Ante__Onboarding__EventStore"];
    }

    [Fact] void should_give_each_its_own_ante_store() => _first.ShouldNotEqual(_second);
}
#endif
