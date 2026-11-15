// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyValidator;

public class when_the_key_backups_share_the_bucket_of_the_data : Specification
{
    IReadOnlyList<UnderstudyProblem> _problems;

    void Because()
    {
        var spec = UnderstudySamples.Spec();
        spec.Keys = UnderstudySamples.Keys();
        spec.Keys.Storage.Bucket = spec.Storage.Bucket;
        _problems = UnderstudyValidator.Validate(spec);
    }

    [Fact] void should_refuse() => _problems.Select(problem => problem.Reason).ShouldContain("KeysBucketShared");
}
#endif
