// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Rendering;

/// <summary>
/// Represents an image embedded in an email. The HTML refers to it as <c>cid:{Name}</c>.
/// </summary>
/// <param name="Name">The file name, which is also what the HTML refers to.</param>
/// <param name="MediaType">The media type, such as <c>image/png</c>.</param>
/// <param name="Content">The bytes of the image.</param>
public record EmailImage(string Name, string MediaType, ReadOnlyMemory<byte> Content);
