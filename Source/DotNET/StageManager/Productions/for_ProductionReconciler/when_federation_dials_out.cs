// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_federation_dials_out : a_production_reconciliation
{
    const string Code = "e2e-enrollment-code";
    const string Prefix = "Cratis__Chronicle__Federation__";

    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Federation.Dial = new()
        {
            Enabled = true,
            EventStore = "ledger",
            LogicalSource = "direct",
            Serve = true,
            ServeEventTypes = ["LedgerClosed"],
            EnrollmentCodeSecretRef = new() { Name = "direct-enrollment" }
        };
        return spec;
    }

    void Establish()
    {
        _cluster.StatefulSetsReady = true;
        _cluster.SecretValues["apps/direct-enrollment/code"] = Code;
    }

    Task Because() => Reconcile();

    [Fact] void should_be_ready() => _phase.ShouldEqual(ProductionPhase.Ready);
    [Fact] void should_dial() => Environment($"{Prefix}Dial__Enabled")!.Value.ShouldEqual("true");
    [Fact] void should_name_the_event_store() => Environment($"{Prefix}Dial__EventStore")!.Value.ShouldEqual("ledger");
    [Fact] void should_name_the_logical_source() => Environment($"{Prefix}Dial__LogicalSource")!.Value.ShouldEqual("direct");
    [Fact] void should_serve_its_outbox() => Environment($"{Prefix}Dial__Serve")!.Value.ShouldEqual("true");
    [Fact] void should_limit_what_it_serves() => Environment($"{Prefix}Dial__ServeEventTypes__0")!.Value.ShouldEqual("LedgerClosed");
    [Fact] void should_read_the_enrollment_code_from_a_file() => Environment($"{Prefix}Enrollment__CodePath")!.Value.ShouldEqual($"/run/secrets/chronicle/{Prefix}Enrollment__Code");
    [Fact] void should_mount_the_enrollment_secret() => Chronicle!.Spec.Template.Spec.Volumes.Any(volume => volume.Projected?.Sources.Any(source => source.Secret.Name == "direct-enrollment") == true).ShouldBeTrue();
    [Fact] void should_keep_the_code_out_of_the_statefulset() => JsonSerializer.Serialize(Chronicle).Contains(Code, StringComparison.Ordinal).ShouldBeFalse();
    [Fact] void should_open_no_listener() => Environment($"{Prefix}Enabled").ShouldBeNull();
    [Fact] void should_open_no_port() => ChronicleContainer.Ports.Any(port => port.Name == "federation").ShouldBeFalse();
    [Fact] void should_add_no_federation_port_to_the_service() => _cluster.AppliedNamed<V1Service>("ledger-chronicle")!.Spec.Ports.Any(port => port.Name == "federation").ShouldBeFalse();
}
#endif
