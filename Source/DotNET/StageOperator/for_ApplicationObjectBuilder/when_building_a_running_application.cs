// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using k8s.Models;

namespace Cratis.Showrunner.StageOperator.for_ApplicationObjectBuilder;

public class when_building_a_running_application : Specification
{
    DesiredObjects _result = null!;
    V1PodSpec _pod = null!;

    void Because()
    {
        _result = ApplicationObjectBuilder.Build(Applications.Create(), Applications.NewImage, 1, true, new StageOperatorOptions { IngressClassName = "public", TlsIssuerAnnotation = "letsencrypt" });
        _pod = _result.Deployment.Spec.Template.Spec;
    }

    [Fact] void should_run_a_stage_and_a_proxy_container() => Assert.Equal(["stage", "auth-proxy"], _pod.Containers.Select(c => c.Name));
    [Fact] void should_run_the_exported_image_in_stage() => Assert.Equal(Applications.NewImage, _pod.Containers[0].Image);
    [Fact] void should_run_stage_in_published_mode() => Assert.Contains(_pod.Containers[0].Env, e => e.Name == "Stage__Mode" && e.Value == "Published");
    [Fact] void should_trust_the_proxy_for_authentication() => Assert.Contains(_pod.Containers[0].Env, e => e.Name == "Stage__Published__Authentication__Mode" && e.Value == "TrustedProxy");
    [Fact] void should_keep_notes_off_by_default() => Assert.Contains(_pod.Containers[0].Env, e => e.Name == "Stage__Published__Notes__Enabled" && e.Value == "false");
    [Fact] void should_trust_principal_headers_only_from_the_proxy_in_the_pod() => Assert.Contains(_pod.Containers[0].Env, e => e.Name == "Stage__Published__TrustedProxies__0" && e.Value == "127.0.0.1");
    [Fact] void should_only_allow_the_published_host() => Assert.Contains(_pod.Containers[0].Env, e => e.Name == "AllowedHosts" && e.Value == "orders.cratis.app");
    [Fact] void should_point_stage_at_the_application_database() => Assert.Contains(_pod.Containers[0].Env, e => e.Name == "Stage__Published__Storage__ConnectionString" && e.Value == "mongodb://orders-mongo:27017");
    [Fact] void should_configure_the_proxy_from_its_secret() => Assert.Equal("orders-auth", _pod.Containers[1].EnvFrom.Single().SecretRef.Name);
    [Fact] void should_default_the_stage_requests() => Assert.Equal("250m", _pod.Containers[0].Resources.Requests["cpu"].ToString());
    [Fact] void should_default_the_stage_limits() => Assert.Equal("1Gi", _pod.Containers[0].Resources.Limits["memory"].ToString());
    [Fact] void should_run_one_replica() => Assert.Equal(1, _result.Deployment.Spec.Replicas);
    [Fact] void should_surge_before_removing_the_old_pod() => Assert.Equal(0, int.Parse(_result.Deployment.Spec.Strategy.RollingUpdate.MaxUnavailable.Value));
    [Fact] void should_not_mount_the_service_account_token() => Assert.False(_pod.AutomountServiceAccountToken);
    [Fact] void should_own_every_object_through_the_resource() => Assert.All(_result.All, o => Assert.Equal("StageApplication", Assert.Single(o.Metadata.OwnerReferences).Kind));
    [Fact] void should_label_every_object_with_the_publication() => Assert.All(_result.All, o => Assert.Equal("22222222-2222-2222-2222-222222222222", o.Metadata.Labels[ApplicationNames.PublicationLabel]));
    [Fact] void should_sanitize_the_organization_label() => Assert.Equal("Example_Org", _result.Deployment.Metadata.Labels[ApplicationNames.OrganizationLabel]);
    [Fact] void should_create_the_database_volume_with_the_default_size() => Assert.Equal("5Gi", _result.MongoStatefulSet.Spec.VolumeClaimTemplates.Single().Spec.Resources.Requests["storage"].ToString());
    [Fact] void should_name_the_database_after_the_application() => Assert.Equal("orders-mongo", _result.MongoStatefulSet.Metadata.Name);
}
#endif
