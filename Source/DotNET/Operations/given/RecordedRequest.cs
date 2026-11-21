// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An HTTP request a <see cref="StubHttpHandler"/> saw.
/// </summary>
/// <param name="Method">The method.</param>
/// <param name="Uri">The URI.</param>
/// <param name="Authorization">The Authorization header.</param>
/// <param name="UserAgent">The User-Agent header.</param>
/// <param name="Body">The body.</param>
public sealed record RecordedRequest(HttpMethod Method, Uri Uri, string? Authorization, string UserAgent, string Body);
#endif
