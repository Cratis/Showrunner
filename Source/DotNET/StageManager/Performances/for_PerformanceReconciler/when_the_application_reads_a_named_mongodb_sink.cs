// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_application_reads_a_named_mongodb_sink : given_a_performance_on_a_production
{
    void Establish() => _performance.Spec.Application.Sinks = ["Documents"];

    Task Because() => Reconcile();

    [Fact] void should_roll_out() => _phase.ShouldEqual(PerformancePhase.Progressing);
    [Fact] void should_have_its_sinks_bound() => Condition("SinksBound").Status.ShouldEqual("True");
    [Fact] void should_have_every_sink_readable() => Condition("SinksReadable").Status.ShouldEqual("True");
    [Fact] void should_name_the_file_for_arc() => Application.Env.Single(variable => variable.Name == "Cratis__MongoDB__Sinks__documents__ServerFile").Value.ShouldEqual("/run/secrets/sinks/documents");
    [Fact] void should_mount_the_sinks_folder() => Application.VolumeMounts.Single(mount => mount.Name == "sinks").MountPath.ShouldEqual("/run/secrets/sinks");
    [Fact] void should_mount_the_key_of_the_sink_secret_as_the_file() => ApplicationDeployment.Spec.Template.Spec.Volumes.Single(volume => volume.Name == "sinks").Projected.Sources.Single().Secret.Items.Single().Key.ShouldEqual("connectionString");
    [Fact] void should_not_put_the_connection_in_the_deployment() => JsonSerializer.Serialize(ApplicationDeployment).Contains(PerformanceSamples.SinkConnectionString, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_not_put_a_secret_value_in_the_status() => PerformanceSamples.SecretValues.Any(StatusAsJson.Contains).ShouldBeFalse();
}
#endif
