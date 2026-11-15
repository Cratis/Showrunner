// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyValidator;

public class when_the_key_backups_are_incomplete : Specification
{
    IReadOnlyList<UnderstudyProblem> _problems;

    void Because()
    {
        var spec = UnderstudySamples.Spec();
        spec.Keys = UnderstudySamples.Keys();
        spec.Keys.CertificateSecret.Name = string.Empty;
        spec.Keys.VaultTokenSecret.Name = string.Empty;
        spec.Keys.MaxAgeDays = 0;
        spec.Keys.Schedule = "nightly";
        _problems = UnderstudyValidator.Validate(spec);
    }

    [Fact] void should_ask_for_the_certificate() => _problems.Select(problem => problem.Reason).ShouldContain("CertificateMissing");
    [Fact] void should_ask_for_the_vault_token() => _problems.Select(problem => problem.Reason).ShouldContain("VaultTokenMissing");
    [Fact] void should_refuse_no_maximum_age() => _problems.Select(problem => problem.Reason).ShouldContain("InvalidKeysMaxAge");
    [Fact] void should_refuse_the_schedule() => _problems.Select(problem => problem.Reason).ShouldContain("InvalidKeysSchedule");
}
#endif
