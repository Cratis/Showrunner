// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Herald.Transport;

/// <summary>
/// Writes each message as an <c>.eml</c> file to the pickup folder, for development: nothing leaves the machine.
/// </summary>
/// <param name="options">The <see cref="HeraldOptions"/>.</param>
public class FolderEmailTransport(IOptions<HeraldOptions> options) : IEmailTransport
{
    /// <inheritdoc/>
    public async Task Send(EmailMessage message, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(options.Value.PickupFolder);
        var path = Path.Combine(options.Value.PickupFolder, $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}.eml");
        await using var stream = File.Create(path);
        using var mime = MimeMessages.From(message);
        await mime.WriteToAsync(stream, cancellationToken);
    }
}
