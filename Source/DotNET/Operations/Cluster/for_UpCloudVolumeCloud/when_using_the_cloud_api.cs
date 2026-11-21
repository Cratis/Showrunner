// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using System.Text;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Cluster.for_UpCloudVolumeCloud;

public class when_using_the_cloud_api : Specification
{
    StubHttpHandler _handler;
    IReadOnlyList<CloudVolume> _volumes;
    IReadOnlyList<CloudAttachment> _attachments;
    bool _stillAttached;
    UpCloudException _failure;

    static HttpResponseMessage Json(string body, HttpStatusCode status = HttpStatusCode.OK) => new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    HttpResponseMessage Answer(HttpRequestMessage request) => (request.Method.Method, request.RequestUri!.AbsolutePath) switch
    {
        ("GET", "/1.3/storage/normal") => Json("{\"storages\":{\"storage\":[{\"uuid\":\"v1\",\"title\":\"pvc-1\",\"created\":\"2026-06-30T10:00:00Z\",\"size\":20,\"labels\":[{\"key\":\"csi_cluster_id\",\"value\":\"mine\"}]},{\"uuid\":\"v2\",\"title\":\"pvc-2\",\"created\":\"2026-06-30T10:00:00Z\",\"size\":5,\"labels\":{\"label\":[{\"key\":\"csi_cluster_id\",\"value\":\"other\"}]}}]}}"),
        ("GET", "/1.3/storage/v1") => Json("{\"storage\":{\"servers\":{\"server\":[\"s1\"]}}}"),
        ("GET", "/1.3/server/s1") => Json("{\"server\":{\"state\":\"started\",\"storage_devices\":{\"storage_device\":[{\"storage\":\"v1\",\"address\":\"virtio:1\"}]}}}"),
        ("POST", "/1.3/server/s1/storage/detach") => Json("{}"),
        ("DELETE", "/1.3/storage/attached") => Json("{\"error\":{\"error_code\":\"STORAGE_ATTACHED\",\"error_message\":\"secret detail\"}}", HttpStatusCode.Conflict),
        _ => Json("{\"error\":{\"error_code\":\"BAD\",\"error_message\":\"secret detail\"}}", HttpStatusCode.InternalServerError)
    };

    async Task Because()
    {
        _handler = new StubHttpHandler(Answer);
        var cloud = new UpCloudVolumeCloud(new HttpClient(_handler), "user", "pw", "csi_cluster_id");
        _volumes = await cloud.ListVolumes(CancellationToken.None);
        _attachments = await cloud.Attachments("v1", CancellationToken.None);
        await cloud.Detach(_attachments[0], CancellationToken.None);
        _stillAttached = !await cloud.Delete("attached", CancellationToken.None);
        try
        {
            await cloud.Delete("broken", CancellationToken.None);
        }
        catch (UpCloudException error)
        {
            _failure = error;
        }
    }

    [Fact] void should_read_the_volumes_with_labels_in_either_shape() => _volumes.Select(volume => volume.ClusterId).ShouldContainOnly("mine", "other");
    [Fact] void should_read_the_size_and_time() => (_volumes[0].SizeGigabytes, _volumes[0].Created).ShouldEqual((20, new DateTimeOffset(2026, 6, 30, 10, 0, 0, TimeSpan.Zero)));
    [Fact] void should_find_the_server_and_the_device_address() => _attachments.Single().ShouldEqual(new CloudAttachment("s1", true, "virtio:1"));
    [Fact] void should_detach_with_the_device_address() => _handler.Requests.Single(request => request.Uri.AbsolutePath.EndsWith("detach", StringComparison.Ordinal)).Body.ShouldContain("virtio:1");
    [Fact] void should_say_a_volume_is_still_attached_rather_than_fail() => _stillAttached.ShouldBeTrue();
    [Fact] void should_send_basic_credentials() => _handler.Requests[0].Authorization.ShouldEqual("Basic dXNlcjpwdw==");
    [Fact] void should_fail_with_the_status_and_not_the_body() => _failure.Message.ShouldEqual("DELETE storage/broken: HTTP 500");
    [Fact] void should_never_put_the_response_body_in_an_error() => _failure.Message.ShouldNotContain("secret detail");
}
#endif
