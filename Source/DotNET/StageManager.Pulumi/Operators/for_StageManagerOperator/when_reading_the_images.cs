// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
namespace Cratis.Showrunner.StageManager.Pulumi.Operators.for_StageManagerOperator;

public class when_reading_the_images : Specification
{
    StageManagerOperatorArgs _published;
    StageManagerOperatorArgs _mirrored;

    void Establish()
    {
        _published = new StageManagerOperatorArgs { Version = "2.17.1" };
        _mirrored = new StageManagerOperatorArgs { Version = "2.17.1", WatchdogImageRepository = "mirror.example.com/watchdog" };
    }

    [Fact] void should_tag_the_operator_image_with_the_version() => StageManagerOperatorValues.OperatorImage(_published).ShouldEqual("cratis/showrunner-operator-image:2.17.1");
    [Fact] void should_tag_the_understudy_image_with_the_version() => StageManagerOperatorValues.UnderstudyImage(_published).ShouldEqual("cratis/showrunner-understudy-image:2.17.1");
    [Fact] void should_tag_the_watchdog_image_with_the_same_version() => StageManagerOperatorValues.WatchdogImage(_published).ShouldEqual("cratis/showrunner-watchdog-image:2.17.1");
    [Fact] void should_use_the_watchdog_repository_override() => StageManagerOperatorValues.WatchdogImage(_mirrored).ShouldEqual("mirror.example.com/watchdog:2.17.1");
}
#endif
