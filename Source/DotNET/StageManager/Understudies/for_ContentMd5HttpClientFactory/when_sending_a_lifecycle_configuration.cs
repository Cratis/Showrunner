// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using System.Text;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_ContentMd5HttpClientFactory;

public class when_sending_a_lifecycle_configuration : Specification
{
    const string Body = "<LifecycleConfiguration><Rule><ID>expiry</ID></Rule></LifecycleConfiguration>";

    RecordingHandler _recorder;
    HttpMessageInvoker _invoker;
    byte[] _md5;
    byte[] _otherMd5;

    void Establish()
    {
        _recorder = new();
        _invoker = new(new ContentMd5Handler(_recorder));
    }

    async Task Because()
    {
        using var lifecycle = new HttpRequestMessage(HttpMethod.Put, "https://storage.example/bucket?lifecycle") { Content = new StringContent(Body, Encoding.UTF8, "application/xml") };
        await _invoker.SendAsync(lifecycle, CancellationToken.None);
        _md5 = _recorder.Md5;

        using var other = new HttpRequestMessage(HttpMethod.Put, "https://storage.example/bucket/key") { Content = new StringContent(Body, Encoding.UTF8, "application/xml") };
        await _invoker.SendAsync(other, CancellationToken.None);
        _otherMd5 = _recorder.Md5;
    }

    [Fact] void should_add_the_md5_of_the_body_to_the_lifecycle_request() => _md5.ShouldEqual(MD5.HashData(Encoding.UTF8.GetBytes(Body)));
    [Fact] void should_leave_other_requests_alone() => _otherMd5.ShouldBeNull();

    sealed class RecordingHandler : HttpMessageHandler
    {
        public byte[] Md5 { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Md5 = request.Content?.Headers.ContentMD5;
            return Task.FromResult(new HttpResponseMessage());
        }
    }
}
#endif
