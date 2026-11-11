// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// The request and purpose an AuthProxy call attests to.
/// </summary>
/// <param name="Purpose">The purpose of the assertion.</param>
/// <param name="Method">The HTTP method.</param>
/// <param name="Target">The target of the call.</param>
/// <param name="Body">The exact request body bytes.</param>
sealed record AuthProxyRequestBinding(string Purpose, HttpMethod Method, Uri Target, byte[] Body);
