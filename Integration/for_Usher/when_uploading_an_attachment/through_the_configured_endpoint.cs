// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Text.Json;
using Cratis.Showrunner.Usher.Integration.given;

namespace Cratis.Showrunner.Usher.Integration.for_Usher.when_uploading_an_attachment;

/// <summary>
/// A reporter pastes or drops a file while writing a report. The endpoint behind Usher's report dialogs accepts
/// an allow-listed image under the configured size limit, refuses an oversized upload and an unauthenticated one,
/// and serves anything outside the inline allow-list - SVG included - as a forced download rather than inline,
/// so an uploaded file can never run as a page or a script in the application's own origin.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class through_the_configured_endpoint : Specification
{
    static readonly ForwardedUser _reporter = new("entra", "reporter-one", "Ada Lovelace", ["anonymous", "authenticated"], [new("name", "Ada Lovelace")]);

    AuthProxy _authProxy;
    Desk _desk;
    WebApplicationWithUsher _application;

    HttpStatusCode _imageStatus;
    JsonElement _imageUploaded;
    HttpResponseMessage _imageServed;
    HttpStatusCode _svgStatus;
    JsonElement _svgUploaded;
    HttpResponseMessage _svgServed;
    HttpStatusCode _oversizedStatus;
    HttpStatusCode _unauthenticatedStatus;

    async Task Because()
    {
        var infrastructure = ChronicleInfrastructure.Current;
        _authProxy = new();
        _desk = await Desk.Connect(infrastructure);

        // A ten-byte limit, so "oversized" needs no real multi-megabyte payload to prove.
        _application = await WebApplicationWithUsher.Start(infrastructure, _desk.Name, _authProxy, maxAttachmentSizeInBytes: 10);

        using (var response = await _application.UploadAttachment("screenshot.png", "image/png", [1, 2, 3, 4, 5], _reporter))
        {
            _imageStatus = response.StatusCode;
            _imageUploaded = JsonElement.Parse(await response.Content.ReadAsStringAsync());
        }

        _imageServed = await _application.Http.GetAsync(new Uri(_imageUploaded.GetProperty("url").GetString()!).PathAndQuery);

        using (var response = await _application.UploadAttachment("diagram.svg", "image/svg+xml", [1, 2, 3, 4, 5], _reporter))
        {
            _svgStatus = response.StatusCode;
            _svgUploaded = JsonElement.Parse(await response.Content.ReadAsStringAsync());
        }

        _svgServed = await _application.Http.GetAsync(new Uri(_svgUploaded.GetProperty("url").GetString()!).PathAndQuery);

        using (var oversized = await _application.UploadAttachment("log.txt", "text/plain", new byte[20], _reporter))
        {
            _oversizedStatus = oversized.StatusCode;
        }

        using var unauthenticated = await _application.UploadAttachment("screenshot.png", "image/png", [1, 2, 3], null);
        _unauthenticatedStatus = unauthenticated.StatusCode;
    }

    void Destroy()
    {
        _imageServed?.Dispose();
        _svgServed?.Dispose();
        _application?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _desk?.Dispose();
        _authProxy?.Dispose();
    }

    [Fact] void should_accept_the_allow_listed_image() => _imageStatus.ShouldEqual(HttpStatusCode.OK);
    [Fact] void should_say_the_image_renders_inline() => _imageUploaded.GetProperty("isImage").GetBoolean().ShouldBeTrue();
    [Fact] void should_give_the_image_an_absolute_url() => _imageUploaded.GetProperty("url").GetString().ShouldMatch(_ => Uri.IsWellFormedUriString(_, UriKind.Absolute));
    [Fact] void should_serve_the_image_inline() => _imageServed.Content.Headers.ContentDisposition.ShouldBeNull();
    [Fact] void should_serve_the_image_with_its_content_type() => _imageServed.Content.Headers.ContentType!.MediaType.ShouldEqual("image/png");

    [Fact] void should_accept_the_svg_upload() => _svgStatus.ShouldEqual(HttpStatusCode.OK);
    [Fact] void should_say_the_svg_does_not_render_inline() => _svgUploaded.GetProperty("isImage").GetBoolean().ShouldBeFalse();
    [Fact] void should_serve_the_svg_as_a_forced_download() => _svgServed.Content.Headers.ContentDisposition!.DispositionType.ShouldEqual("attachment");
    [Fact] void should_never_serve_the_svg_as_its_own_content_type() => _svgServed.Content.Headers.ContentType!.MediaType.ShouldEqual("application/octet-stream");

    [Fact] void should_refuse_an_upload_over_the_configured_limit() => _oversizedStatus.ShouldEqual(HttpStatusCode.BadRequest);
    [Fact] void should_refuse_an_unauthenticated_upload() => _unauthenticatedStatus.ShouldEqual(HttpStatusCode.Unauthorized);
}
