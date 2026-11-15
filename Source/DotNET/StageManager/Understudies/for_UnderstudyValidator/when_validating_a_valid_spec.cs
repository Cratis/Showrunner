// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyValidator;

public class when_validating_a_valid_spec : Specification
{
    IReadOnlyList<UnderstudyProblem> _problems;

    void Because() => _problems = UnderstudyValidator.Validate(UnderstudySamples.Spec());

    [Fact] void should_find_no_problem() => _problems.ShouldBeEmpty();
}
#endif
