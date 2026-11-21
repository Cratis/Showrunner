// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Channels;

/// <summary>
/// Where and as whom alerts are pushed to Cratis Direct.
/// </summary>
/// <param name="BaseUrl">The base URL of the Direct, for example <c>https://direct.example.com</c>.</param>
/// <param name="Token">The bearer token of the registered alert source. It is never logged.</param>
/// <param name="Source">The name of the source to record the alerts under. When <see langword="null"/>, Direct uses the name of the source the token belongs to.</param>
public sealed record DirectAlertsOptions(Uri BaseUrl, string Token, string? Source = null);
