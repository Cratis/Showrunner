// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyJobConfiguration;

public class when_the_system_database_is_named : Specification
{
    UnderstudyJobConfiguration _named;
    UnderstudyJobConfiguration _unnamed;

    void Because()
    {
        var production = UnderstudySamples.Production();
        production.Spec.Database.SystemDatabaseName = "System";
        _named = UnderstudyWorkload.Configuration(UnderstudySamples.Understudy(UnderstudySamples.Spec()), production, UnderstudyJobKinds.Backup);
        _unnamed = UnderstudyWorkload.Configuration(UnderstudySamples.Understudy(UnderstudySamples.Spec()), UnderstudySamples.Production(), UnderstudyJobKinds.Backup);
    }

    [Fact] void should_tell_the_job_the_name() => _named.SystemDatabaseName.ShouldEqual("System");
    [Fact] void should_leave_the_default_when_no_name_is_given() => _unnamed.SystemDatabaseName.ShouldBeEmpty();
}
#endif
