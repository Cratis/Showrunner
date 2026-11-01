// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_SpecValidator;

public class when_validating : Specification
{
    IReadOnlyList<SpecProblem> _valid = null!;
    IReadOnlyList<SpecProblem> _withoutAuthentication = null!;
    IReadOnlyList<SpecProblem> _withoutSecret = null!;
    IReadOnlyList<SpecProblem> _stagePort = null!;
    IReadOnlyList<SpecProblem> _bothLifetimes = null!;
    IReadOnlyList<SpecProblem> _noLifetime = null!;
    IReadOnlyList<SpecProblem> _badUntil = null!;
    IReadOnlyList<SpecProblem> _badName = null!;
    IReadOnlyList<SpecProblem> _nameMismatch = null!;
    IReadOnlyList<SpecProblem> _timeLimited = null!;

    void Because()
    {
        _valid = SpecValidator.Validate(Applications.Create());
        _withoutAuthentication = SpecValidator.Validate(Applications.Create(withAuthentication: false));

        var noSecret = Applications.Create();
        noSecret.Spec = noSecret.Spec with { Authentication = noSecret.Spec.Authentication! with { ConfigSecretRef = " " } };
        _withoutSecret = SpecValidator.Validate(noSecret);

        var stagePort = Applications.Create();
        stagePort.Spec = stagePort.Spec with { Authentication = stagePort.Spec.Authentication! with { Port = 9090 } };
        _stagePort = SpecValidator.Validate(stagePort);

        var both = Applications.Create();
        both.Spec = both.Spec with { Lifetime = new StageApplicationLifetime { Permanent = true, Until = "2027-01-01T00:00:00Z" } };
        _bothLifetimes = SpecValidator.Validate(both);

        var none = Applications.Create();
        none.Spec = none.Spec with { Lifetime = null };
        _noLifetime = SpecValidator.Validate(none);

        var badUntil = Applications.Create();
        badUntil.Spec = badUntil.Spec with { Lifetime = new StageApplicationLifetime { Until = "tomorrow-ish" } };
        _badUntil = SpecValidator.Validate(badUntil);

        var badName = Applications.Create();
        badName.Spec = badName.Spec with { PublishedName = "Orders_1" };
        _badName = SpecValidator.Validate(badName);

        var mismatch = Applications.Create();
        mismatch.Spec = mismatch.Spec with { PublishedName = "invoices" };
        _nameMismatch = SpecValidator.Validate(mismatch);

        _timeLimited = SpecValidator.Validate(Applications.Create(permanent: false));
    }

    [Fact] void should_accept_a_valid_application() => Assert.Empty(_valid);
    [Fact] void should_accept_a_time_limited_application() => Assert.Empty(_timeLimited);
    [Fact] void should_refuse_a_missing_authentication_block() => Assert.Equal(SpecValidator.AuthenticationMissing, Assert.Single(_withoutAuthentication).Reason);
    [Fact] void should_refuse_an_empty_proxy_secret() => Assert.Equal(SpecValidator.AuthenticationMissing, Assert.Single(_withoutSecret).Reason);
    [Fact] void should_refuse_a_proxy_on_the_stage_port() => Assert.Equal(SpecValidator.InvalidSpec, Assert.Single(_stagePort).Reason);
    [Fact] void should_refuse_both_permanent_and_until() => Assert.Single(_bothLifetimes);
    [Fact] void should_refuse_no_lifetime() => Assert.Single(_noLifetime);
    [Fact] void should_refuse_an_until_that_is_not_a_timestamp() => Assert.Single(_badUntil);
    [Fact] void should_refuse_a_published_name_that_is_not_a_dns_label() => Assert.Single(_badName);
    [Fact] void should_refuse_a_name_that_differs_from_the_published_name() => Assert.Single(_nameMismatch);
}
#endif
