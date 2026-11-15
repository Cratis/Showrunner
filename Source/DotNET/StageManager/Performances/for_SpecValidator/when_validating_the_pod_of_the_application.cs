// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_SpecValidator;

public class when_validating_the_pod_of_the_application : Specification
{
    IReadOnlyList<string> _unknownStrategy;
    IReadOnlyList<string> _relativeProbe;
    IReadOnlyList<string> _volumes;
    IReadOnlyList<string> _initContainers;
    IReadOnlyList<string> _resources;
    IReadOnlyList<string> _egress;
    IReadOnlyList<string> _published;
    IReadOnlyList<string> _valid;

    void Because()
    {
        _unknownStrategy = Validate(application => application.Strategy = "Blue");
        _relativeProbe = Validate(application => application.Probes = new() { LivenessPath = "health" });
        _volumes = Validate(application =>
        {
            application.Volumes = [new() { Name = "Bad_Name" }, new() { Name = "data", Kind = ApplicationVolume.PersistentVolumeClaim }, new() { Name = "data" }];
            application.VolumeMounts = [new() { Volume = "missing", Path = "relative" }];
        });
        _initContainers = Validate(application => application.InitContainers = [new() { Name = "prepare", VolumeMounts = [new() { Volume = "missing", Path = "/x" }] }, new() { Name = "prepare" }]);
        _resources = Validate(application => application.Resources = new() { Requests = new Dictionary<string, string> { ["memory"] = "lots" } });
        _egress = Validate(application => application.Egress =
        [
            new() { Ports = [] },
            new() { Cidr = "10.0.0.0/8", Namespace = "other", Ports = [443] },
            new() { Cidr = "not-a-range", Ports = [443] },
            new() { Ports = [70000] }
        ]);
        _published = Validate(application => application.Published = PerformanceSamples.PublishedPerformance().Spec.Application.Published, spec => spec.Application.Strategy = ApplicationSpec.Recreate, spec => spec.Application.InitContainers = [new() { Name = "prepare" }]);
        _valid = Validate(application =>
        {
            application.Strategy = ApplicationSpec.Recreate;
            application.Probes = new() { ReadinessPath = "/ready", StartupPath = "/started" };
            application.Volumes = [new() { Name = "trust" }];
            application.VolumeMounts = [new() { Volume = "trust", Path = "/trust" }];
            application.InitContainers = [new() { Name = "prepare", VolumeMounts = [new() { Volume = "trust", Path = "/trust" }] }];
            application.Resources = new() { Requests = new Dictionary<string, string> { ["cpu"] = "250m" } };
            application.Egress = [new() { Ports = [587] }, new() { Cidr = "203.0.113.0/24", Ports = [443] }];
        });
    }

    static IReadOnlyList<string> Validate(Action<ApplicationSpec> change, params Action<PerformanceSpec>[] more)
    {
        var performance = PerformanceSamples.Performance();
        change(performance.Spec.Application);
        foreach (var action in more)
        {
            action(performance.Spec);
        }

        return SpecValidator.Validate(performance);
    }

    [Fact] void should_refuse_an_unknown_strategy() => _unknownStrategy.Single().ShouldContain("application.strategy must be RollingUpdate or Recreate");
    [Fact] void should_refuse_a_probe_path_that_is_not_absolute() => _relativeProbe.Single().ShouldContain("application.probes.livenessPath must start with /");
    [Fact] void should_refuse_a_volume_name_that_is_not_a_dns_label() => _volumes.Any(problem => problem.Contains("'Bad_Name' must be a DNS label", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_a_claim_without_a_name() => _volumes.Any(problem => problem.Contains("needs claimName", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_a_volume_named_twice() => _volumes.Any(problem => problem.Contains("names 'data' twice", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_a_mount_of_an_undeclared_volume() => _volumes.Any(problem => problem.Contains("mounts the volume 'missing'", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_a_mount_path_that_is_not_absolute() => _volumes.Any(problem => problem.Contains("path 'relative' must be absolute", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_an_init_container_named_twice() => _initContainers.Any(problem => problem.Contains("names 'prepare' twice", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_an_init_container_mount_of_an_undeclared_volume() => _initContainers.Any(problem => problem.Contains("initContainers 'prepare' volumeMounts mounts the volume 'missing'", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_a_quantity_that_is_not_one() => _resources.Single().ShouldContain("application.resources.requests.memory 'lots'");
    [Fact] void should_refuse_a_rule_without_ports() => _egress.Any(problem => problem.StartsWith("application.egress[0].ports", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_a_rule_with_a_cidr_and_a_namespace() => _egress.Any(problem => problem.Contains("egress[1] names both cidr and namespace", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_a_cidr_that_is_not_a_range() => _egress.Any(problem => problem.Contains("'not-a-range' is not an address range", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_a_port_out_of_range() => _egress.Any(problem => problem.StartsWith("application.egress[3].ports", StringComparison.Ordinal)).ShouldBeTrue();
    [Fact] void should_refuse_pod_settings_of_a_published_application() => _published.Count(problem => problem.Contains("does not apply to a published application", StringComparison.Ordinal)).ShouldEqual(2);
    [Fact] void should_accept_a_valid_pod() => _valid.ShouldBeEmpty();
}
#endif
