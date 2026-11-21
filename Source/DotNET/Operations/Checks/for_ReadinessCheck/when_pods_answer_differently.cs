// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Net.Sockets;
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_ReadinessCheck;

public class when_pods_answer_differently : a_check
{
    StubHttpHandler _handler;

    protected override IWatchdogCheck Check => new ReadinessCheck(
        _reader,
        new HttpClient(_handler),
        new ReadinessCheckOptions { Namespace = "core", LabelSelector = "app=core", ContainerName = "core", HangAdvice = "delete the pod" },
        _time);

    void Establish()
    {
        PodInfo Pod(string name, string ip, string phase = "Running", ContainerInfo? container = null) =>
            new("core", name, phase, _time.Now.AddDays(-1), [container ?? new ContainerInfo("core")], PodIp: ip, Labels: new Dictionary<string, string> { ["app"] = "core" });

        _reader.Pods.AddRange([
            Pod("healthy", "10.0.0.1"),
            Pod("unready", "10.0.0.2"),
            Pod("refused", "10.0.0.3"),
            Pod("hung", "10.0.0.4"),
            Pod("no-ip", null!),
            Pod("pending", "10.0.0.5", "Pending"),
            Pod("crash-looping", "10.0.0.6", container: new ContainerInfo("core", true, 4)),
            Pod("not-ready-container", "10.0.0.7", container: new ContainerInfo("core", false))]);

        _handler = new StubHttpHandler(request => request.RequestUri!.Host switch
        {
            "10.0.0.2" => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            "10.0.0.3" => throw new HttpRequestException("refused", new SocketException((int)SocketError.ConnectionRefused)),
            "10.0.0.4" => throw new TaskCanceledException("timed out"),
            _ => new HttpResponseMessage(HttpStatusCode.OK)
        });
    }

    async Task Because() => await Run();

    [Fact] void should_probe_each_pod_by_ip_on_the_readiness_path() => _handler.Requests.Select(request => request.Uri.ToString()).ShouldContainOnly("http://10.0.0.1:8080/health/ready", "http://10.0.0.2:8080/health/ready", "http://10.0.0.3:8080/health/ready", "http://10.0.0.4:8080/health/ready");
    [Fact] void should_not_report_a_pod_that_answers() => Keys.Any(key => key.Contains("healthy")).ShouldBeFalse();
    [Fact] void should_report_the_status_of_a_pod_that_answers_with_an_error() => _findings.First(finding => finding.Key == "pod:core/unready:NotReady").Text.ShouldContain("HTTP 503");
    [Fact] void should_tell_a_refusal_from_a_hang() => Keys.ShouldContain("pod:core/refused:ReadinessRefused");
    [Fact] void should_say_that_a_refusal_is_likely_a_pod_that_is_still_starting() => _findings.First(finding => finding.Key == "pod:core/refused:ReadinessRefused").Text.ShouldContain("rather than wedged");
    [Fact] void should_report_a_pod_that_never_answers_as_hung() => Keys.ShouldContain("pod:core/hung:ReadinessHang");
    [Fact] void should_give_the_advice_for_a_hang() => _findings.First(finding => finding.Key == "pod:core/hung:ReadinessHang").Text.ShouldContain("delete the pod");
    [Fact] void should_not_probe_a_running_pod_without_an_ip() => Keys.Any(key => key.Contains("no-ip")).ShouldBeFalse();
    [Fact] void should_not_probe_a_pod_that_is_not_running() => Keys.Any(key => key.Contains("pending")).ShouldBeFalse();
    [Fact] void should_leave_a_crash_looping_container_to_its_own_finding() => Keys.Any(key => key.Contains("crash-looping")).ShouldBeFalse();
    [Fact] void should_leave_a_container_that_is_not_ready_to_its_own_finding() => Keys.Any(key => key.Contains("not-ready-container")).ShouldBeFalse();
    [Fact] void should_send_no_identity() => _handler.Requests.TrueForAll(request => request.Authorization is null).ShouldBeTrue();
    [Fact] void should_only_look_at_the_pods_of_the_workload() => _reader.PodNamespaces.ShouldContainOnly("core");
}
#endif
