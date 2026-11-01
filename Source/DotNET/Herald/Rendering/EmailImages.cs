// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.Rendering;

/// <summary>
/// Finds the images an email refers to as <c>cid:{file name}</c> in the images folder.
/// </summary>
/// <remarks>
/// PNG, JPEG and GIF are embedded in the email. SVG is refused with an error naming the file: mail clients do not
/// render it (Outlook and Gmail show a broken image), and converting it needs a rasterizer with native libraries, which
/// Herald does not take on. Export a PNG of the image next to the SVG and refer to that.
/// </remarks>
/// <param name="options">The <see cref="HeraldOptions"/>.</param>
public partial class EmailImages(IOptions<HeraldOptions> options)
{
    static readonly Dictionary<string, string> _mediaTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif"
    };

    [GeneratedRegex(@"cid:(?<name>[A-Za-z0-9][A-Za-z0-9_.\-]*)", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex References { get; }

    /// <summary>
    /// Finds the images an email's HTML refers to.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns>The images, each once.</returns>
    /// <exception cref="EmailCannotBeRendered">An image is not in the folder, is an SVG, or is not an image type Herald embeds.</exception>
    public IReadOnlyList<EmailImage> ReferencedBy(string html)
    {
        List<EmailImage> images = [];
        foreach (var name in References.Matches(html).Select(match => match.Groups["name"].Value).Distinct(StringComparer.Ordinal))
        {
            images.Add(Find(name));
        }

        return images;
    }

    EmailImage Find(string name)
    {
        var extension = System.IO.Path.GetExtension(name);
        if (extension.Equals(".svg", StringComparison.OrdinalIgnoreCase))
        {
            throw new EmailCannotBeRendered($"The image '{name}' is an SVG, which mail clients do not render: export a PNG and refer to that.");
        }

        if (!_mediaTypes.TryGetValue(extension, out var mediaType))
        {
            throw new EmailCannotBeRendered($"The image '{name}' is not a PNG, JPEG or GIF.");
        }

        var path = System.IO.Path.Combine(options.Value.ImagesFolder, name);
        if (options.Value.ImagesFolder.Length == 0 || !File.Exists(path))
        {
            throw new EmailCannotBeRendered($"The image '{name}' is not in the images folder.");
        }

        return new(name, mediaType, File.ReadAllBytes(path));
    }
}
