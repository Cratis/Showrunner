// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ApplicationObjectBuilder;

public class when_notes_is_enabled : Specification
{
    DesiredObjects _result = null!;

    void Because()
    {
        var application = Applications.Create();
        application.Spec = application.Spec with { Notes = new StageApplicationNotes { Enabled = true, ConfigSecretRef = "orders-notes" } };
        _result = ApplicationObjectBuilder.Build(application, Applications.NewImage, 1, true, new StageOperatorOptions { NotesCollectorPort = 4318 });
    }

    [Fact] void should_enable_notes_in_stage() => Assert.Contains(_result.Deployment.Spec.Template.Spec.Containers[0].Env, e => e.Name == "Stage__Published__Notes__Enabled" && e.Value == "true");
    [Fact] void should_pass_the_notes_secret_to_stage() => Assert.Equal("orders-notes", _result.Deployment.Spec.Template.Spec.Containers[0].EnvFrom.Single().SecretRef.Name);
    [Fact] void should_allow_the_collector_port_into_the_pod() => Assert.Equal([8080, 4318], _result.NetworkPolicies[0].Spec.Ingress.Single().Ports.Select(p => int.Parse(p.Port.Value)));
    [Fact] void should_still_expose_only_the_proxy_on_the_service() => Assert.Single(_result.Service.Spec.Ports);
}
#endif
