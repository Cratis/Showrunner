// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Releases;

/// <summary>
/// A file a release note refers to.
/// </summary>
/// <param name="Name">The name of the file inside the release's folder.</param>
/// <param name="ContentType">The media type of the file.</param>
/// <param name="Open">Opens the file for reading.</param>
public record ReleaseAsset(string Name, string ContentType, Func<Stream> Open);
